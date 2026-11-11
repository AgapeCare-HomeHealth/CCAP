using CCAP.API.Contracts.Referrals;
using CCAP.API.Authorization;
using CCAP.Application.Features.ReferralDrafts.Queries;
using CCAP.Application.Features.Referrals.Commands.CreateReferralIntake;
using CCAP.Application.Features.Referrals.Commands.CreateReferralTrackerEntries;
using CCAP.Application.Features.Referrals.Commands.SaveReferralDraft;
using CCAP.Application.Features.Referrals.Commands.UpdateReferralTrackerDetails;
using CCAP.Application.Features.Referrals.Queries.GetReferralList;
using CCAP.Application.Abstractions.Referrals;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Infrastructure.Persistence;
using CCAP.Application.Features.ReferralDrafts.Queries.GetReferralDraftById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Concurrent;
using CCAP.API.Hubs;
using CCAP.Domain.Entities;
using Microsoft.AspNetCore.SignalR;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/referrals")]
[Authorize]
public sealed class ReferralController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IReferralDocumentExtractionService _documentExtraction;
    private readonly IUserRepository _users;
    private readonly AppDbContext _db;
    private readonly IHubContext<NotificationsHub> _notificationsHub;
    private static readonly ConcurrentDictionary<Guid, ReferralExtractionProgressResponse> ExtractionProgress = new();

    private const long MaxFileSize =
        10 * 1024 * 1024; // 10 MB

    public ReferralController(
        ISender sender,
        IReferralDocumentExtractionService documentExtraction,
        IUserRepository users, AppDbContext db, IHubContext<NotificationsHub> notificationsHub)
    {
        _sender = sender;
        _documentExtraction = documentExtraction;
        _users = users;
        _db = db;
        _notificationsHub = notificationsHub;
    }

    [HttpGet]
    [Authorize(Policy = PermissionPolicies.ReferralsView)]
    public async Task<IActionResult> GetReferralList(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? source = null,
        [FromQuery] string? insurance = null,
        [FromQuery] Guid? coordinatorId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 100);
        var result = await _sender.Send(
            new GetReferralListQuery(page, size, search, status, source, insurance),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("assignment-users")]
    [Authorize(Policy = PermissionPolicies.ReferralsView)]
    public async Task<IActionResult> GetReferralAssignmentUsers(CancellationToken cancellationToken)
    {
        var users = await _users.GetAllAsync(cancellationToken);
        var coordinators = users
            .Where(user => user.IsActive &&
                !string.IsNullOrWhiteSpace(user.Email) &&
                !string.IsNullOrWhiteSpace(user.PasswordHash) &&
                user.Role is not null && user.Role.IsActive &&
                user.Role.RoleName.Contains("Coordinator", StringComparison.OrdinalIgnoreCase))
            .Select(user => new
            {
                user.UserId, user.EmployeeNo, user.FirstName, user.LastName, user.Email,
                user.RoleId, Role = user.Role.RoleName, user.IsActive
            })
            .OrderBy(user => user.LastName).ThenBy(user => user.FirstName).ToList();
        return Ok(coordinators);
    }

    [HttpPut("{referralId:guid}/tracker")]
    [Authorize(Policy = PermissionPolicies.ReferralsManage)]
    public async Task<IActionResult> UpdateTrackerDetails(
        Guid referralId,
        [FromBody] UpdateReferralTrackerDetailsCommand request,
        CancellationToken cancellationToken)
    {
        if (referralId != request.ReferralId)
            return BadRequest(new { message = "Tracker identifier does not match the request." });
        await _sender.Send(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("tracker")]
    [Authorize(Policy = PermissionPolicies.ReferralsManage)]
    public async Task<IActionResult> CreateTrackerEntries(
        [FromBody] List<CCAP.Application.Features.Referrals.DTOs.CreateReferralTrackerEntryRequest> requests,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CreateReferralTrackerEntriesCommand(requests),
            cancellationToken);
        return Ok(new { saved = result.Saved });
    }

    // SAVE REFERRAL DRAFT
    // =========================================================

    [HttpPost("draft")]
    [Authorize(
    Policy = PermissionPolicies.ReferralsManage)]
    public async Task<IActionResult> SaveDraft(
    [FromBody] SaveReferralDraftRequest request,
    CancellationToken cancellationToken)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
                userIdValue,
                out var currentUserId))
        {
            return Unauthorized(new
            {
                message =
                    "Unable to determine the authenticated user."
            });
        }

        try
        {
            var result =
                await _sender.Send(
                    new SaveReferralDraftCommand(
                        request.ReferralDraftId,
                        currentUserId,
                        request.Data),
                    cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                message = ex.Message
            });
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message = ex.Message,
                    innerException =
                        ex.InnerException?.Message
                });
        }
    }

    // ========================================================
    // EXTRACT REFERRAL PDF
    // ========================================================

    [HttpPost("extract")]
    [Authorize(Policy = PermissionPolicies.ReferralsManage)]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> ExtractReferral(
        IFormFile pdf,
        [FromForm] Guid progressId,
        CancellationToken cancellationToken)
    {
        if (pdf is null || pdf.Length <= 0)
            return BadRequest(new { message = "Please upload a non-empty referral PDF." });
        if (!pdf.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only PDF files are accepted." });
        if (pdf.Length > MaxFileSize)
            return BadRequest(new { message = "Referral PDF cannot exceed 10 MB." });
        if (progressId == Guid.Empty)
            return BadRequest(new { message = "A progress identifier is required." });

        ExtractionProgress[progressId] = new ReferralExtractionProgressResponse { Stage = "Starting extraction" };
        try
        {
            await using Stream stream = pdf.OpenReadStream();
            var progress = new InlineProgress<CCAP.Application.Features.Referrals.DTOs.ReferralExtractionProgress>(p =>
            {
                ExtractionProgress[progressId] = new ReferralExtractionProgressResponse
                {
                    TotalPages = p.TotalPages,
                    PagesProcessed = p.PagesProcessed,
                    Stage = p.Stage
                };
            });

            var result = await _documentExtraction.ExtractAsync(stream, cancellationToken, progress);
            var current = ExtractionProgress.GetValueOrDefault(progressId);
            ExtractionProgress[progressId] = new ReferralExtractionProgressResponse
            {
                TotalPages = current?.TotalPages ?? 0,
                PagesProcessed = current?.TotalPages ?? 0,
                Stage = "Extraction complete",
                IsComplete = true
            };
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            ExtractionProgress.TryRemove(progressId, out _);
            return UnprocessableEntity(new { message = ex.Message });
        }
        catch
        {
            ExtractionProgress.TryRemove(progressId, out _);
            throw;
        }
    }

    [HttpGet("extract/progress/{progressId:guid}")]
    [Authorize(Policy = PermissionPolicies.ReferralsManage)]
    public IActionResult GetExtractionProgress(Guid progressId)
    {
        if (!ExtractionProgress.TryGetValue(progressId, out var progress))
            return Ok(new ReferralExtractionProgressResponse());
        if (progress.IsComplete)
            ExtractionProgress.TryRemove(progressId, out _);
        return Ok(progress);
    }

    // ========================================================
    // GET REFERRAL DRAFTS
    // ========================================================

    [HttpGet("drafts")]
    [Authorize(
    Policy = PermissionPolicies.ReferralsView)]
    public async Task<IActionResult> GetDrafts(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? search = null,
    [FromQuery] string sortBy = "UpdatedAt",
    [FromQuery] bool sortDescending = true,
    CancellationToken cancellationToken = default)
    {
        var result =
            await _sender.Send(
                new GetReferralDraftsQuery(
                    pageNumber,
                    pageSize,
                    search,
                    sortBy,
                    sortDescending),
                cancellationToken);

        return Ok(result);
    }

    [HttpGet("drafts/{draftId:guid}")]
    [Authorize(
    Policy = PermissionPolicies.ReferralsView)]
    public async Task<IActionResult> GetDraft(
    Guid draftId,
    CancellationToken cancellationToken)
    {
        var data =
            await _sender.Send(
                new GetReferralDraftByIdQuery(
                    draftId),
                cancellationToken);

        if (data is null)
        {
            return NotFound(new
            {
                message =
                    "Referral draft was not found."
            });
        }

        return Ok(data);
    }

    // =========================================================
    // CREATE REFERRAL INTAKE
    // =========================================================

    [HttpPost("intake")]
    [Authorize(
    Policy = PermissionPolicies.ReferralsManage)]
    [RequestSizeLimit(MaxFileSize)]
    public async Task<IActionResult> CreateIntake(
    [FromForm] CreateReferralIntakeRequest request,
    CancellationToken cancellationToken)
    {
        // Primary CC defaults to the signed-in user when the client has not
        // supplied a selection. An explicit selection is preserved so the creator
        // can change the Primary CC in the Assignment step.
        if (!request.CoordinatorId.HasValue)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("userId");
            if (Guid.TryParse(userIdValue, out var signedInUserId))
                request.CoordinatorId = signedInUserId;
        }

        // Validate both selected coordinators in the application handler.
        // =========================================================
        // OPTIONAL PDF VALIDATION
        // =========================================================

        if (request.Pdf is not null)
        {
            if (request.Pdf.Length <= 0)
            {
                return BadRequest(new
                {
                    message = "Referral PDF is empty."
                });
            }

            if (request.Pdf.Length > MaxFileSize)
            {
                return BadRequest(new
                {
                    message =
                        "Referral PDF cannot exceed 10 MB."
                });
            }

            // Validate content type
            if (!string.Equals(
                    request.Pdf.ContentType,
                    "application/pdf",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "Only PDF files are allowed."
                });
            }

            // Validate extension
            if (!request.Pdf.FileName.EndsWith(
                    ".pdf",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message =
                        "Only PDF files are allowed."
                });
            }
        }


        if (request.PdfSize.HasValue &&
            (request.PdfSize.Value <= 0 || request.PdfSize.Value > MaxFileSize))
        {
            return BadRequest(new { message = "Referral PDF must be greater than 0 and cannot exceed 10 MB." });
        }


        // =========================================================
        // OPTIONAL PDF STREAM
        // =========================================================

        Stream? stream = null;

        if (request.Pdf is not null)
        {
            stream =
                request.Pdf.OpenReadStream();
        }


        try
        {
            // =====================================================
            // SEND APPLICATION COMMAND
            // =====================================================

            var result = await _sender.Send(
                new CreateReferralIntakeCommand(
                    request.ReferralDraftId,
                    request.MRN ?? string.Empty,
                    request.FirstName,
                    request.MiddleName ?? string.Empty,
                    request.LastName,

                    request.DateOfBirth,
                    request.Gender,

                    request.PrimaryPhone,
                    request.AlternatePhone,

                    request.StreetAddress,
                    request.City,
                    request.State,
                    request.ZipCode,

                    request.EmergencyContactName,
                    request.EmergencyContactRelationship,
                    request.EmergencyContactPhone,
                    request.EmergencyContactsJson,

                    request.ReferralNumber ?? string.Empty,
                    request.ReferralDate,
                    request.ReferralSource,
                    request.ReferralType,
                    request.ReferralChannel,
                    request.Priority,

                    request.PrimaryInsurance,
                    request.InsuranceMemberId,
                    request.AuthorizationDate,
                    request.ApprovedVisits,
                    request.AuthorizationRequired,

                    request.ReferringPhysician,
                    request.PhysicianPhone,
                    request.PhysicianFax,
                    request.PhysicianNpi,
                    request.PcosRegistered,

                    request.PrimaryDiagnosis,
                    request.SecondaryDiagnosis,

                    request.OrderedServices ?? [],

                    request.ReferralNotes,

                    request.CoordinatorId,
                    request.SecondaryCoordinatorId,
                    request.ClinicianId,
                    request.DisciplineId,

                    request.SocDate,
                    request.VisitPriority,
                    request.CaseStatus,

                    request.InternalNotes,

                    // =================================================
                    // OPTIONAL PDF
                    // =================================================

                    stream,

                    request.Pdf?.FileName ?? request.PdfFileName,

                    request.Pdf?.ContentType ?? request.PdfContentType,

                    request.Pdf?.Length ?? request.PdfSize,
                    request.PostedOn,
                    request.TrackerStatus,
                    request.EstimatedDischargeDate,
                    request.TrackerCoordinatorName,
                    request.FirstMarketingCall,
                    request.SecondCoordinatorCall,
                    request.ThirdCoordinatorCall,
                    request.Resolution
                ),
                cancellationToken);


            // Persist workflow assignment notifications only after referral/task
            // creation has committed successfully. SignalR is a delivery channel;
            // the database record remains the durable source of truth.
            var patient = await _db.Patients.AsNoTracking()
                .FirstOrDefaultAsync(x => x.PatientId == result.PatientId, cancellationToken);
            if (patient is not null)
            {
                var roleIds = new[] { patient.CoordinatorId, patient.SecondaryCoordinatorId, patient.BackupCoordinatorId }
                    .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
                var admins = await _db.ApplicationUsers.AsNoTracking()
                    .Where(x => x.IsActive && x.Role.RoleName == "Administrator")
                    .Select(x => x.UserId).ToListAsync(cancellationToken);
                var recipients = roleIds.Concat(admins).Distinct().ToList();
                var primary = patient.CoordinatorId.HasValue
                    ? await _db.ApplicationUsers.Where(x => x.UserId == patient.CoordinatorId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefaultAsync(cancellationToken) : null;
                var secondary = patient.SecondaryCoordinatorId.HasValue
                    ? await _db.ApplicationUsers.Where(x => x.UserId == patient.SecondaryCoordinatorId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefaultAsync(cancellationToken) : null;
                var backup = patient.BackupCoordinatorId.HasValue
                    ? await _db.ApplicationUsers.Where(x => x.UserId == patient.BackupCoordinatorId).Select(x => x.FirstName + " " + x.LastName).FirstOrDefaultAsync(cancellationToken) : null;
                var patientName = $"{patient.FirstName} {patient.LastName}".Trim();
                var details = $"Primary CC: {primary ?? "Unassigned"}; Secondary CC: {secondary ?? "Unassigned"}; Backup CC: {backup ?? "Unassigned"}.";
                var rows = recipients.Select(userId =>
                {
                    var assignment = userId == patient.CoordinatorId ? "Primary CC"
                        : userId == patient.SecondaryCoordinatorId ? "Secondary CC"
                        : userId == patient.BackupCoordinatorId ? "Backup CC" : "Administrator";
                    return new UserNotification(userId, patient.PatientId, patientName,
                        "WorkflowAssignment", "New patient workflow assigned",
                        $"Your assignment: {assignment}. {details}", "Info");
                }).ToList();
                if (rows.Count > 0)
                {
                    await _db.UserNotifications.AddRangeAsync(rows, cancellationToken);
                    await _db.SaveChangesAsync(cancellationToken);
                    foreach (var row in rows)
                        await _notificationsHub.Clients.Group($"user:{row.UserId}")
                            .SendAsync("NotificationCreated", row.NotificationId, cancellationToken);
                }
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        finally
        {
            if (stream is not null)
            {
                await stream.DisposeAsync();
            }
        }
    }
}

internal sealed class InlineProgress<T>(Action<T> report) : IProgress<T>
{
    public void Report(T value) => report(value);
}
