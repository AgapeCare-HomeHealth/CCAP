using CCAP.API.Contracts.Referrals;
using CCAP.API.Authorization;
using CCAP.Application.Features.ReferralDrafts.Queries;
using CCAP.Application.Features.Referrals.Commands.CreateReferralIntake;
using CCAP.Application.Features.Referrals.Commands.SaveReferralDraft;
using CCAP.Application.Features.Referrals.Commands.UpdateReferralTrackerDetails;
using CCAP.Application.Features.Referrals.Queries.GetReferralList;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Abstractions.Referrals;
using CCAP.Application.Features.ReferralDrafts.Queries.GetReferralDraftById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/referrals")]
[Authorize]
public sealed class ReferralController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IReferralDocumentExtractionService _documentExtraction;
    private readonly IUserRepository _users;

    private const long MaxFileSize =
        10 * 1024 * 1024; // 10 MB

    public ReferralController(
        ISender sender,
        IReferralDocumentExtractionService documentExtraction,
        IUserRepository users)
    {
        _sender = sender;
        _documentExtraction = documentExtraction;
        _users = users;
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
        var result = await _sender.Send(new GetReferralListQuery(pageNumber, pageSize, search,
            status, source, insurance, coordinatorId, fromDate, toDate), cancellationToken);
        return Ok(result);
    }

    [HttpGet("assignment-users")]
    [Authorize(Policy = PermissionPolicies.ReferralsView)]
    public async Task<IActionResult> GetReferralAssignmentUsers(
        CancellationToken cancellationToken)
    {
        var users = await _users.GetAllAsync(cancellationToken);

        var coordinators = users
            .Where(user =>
                user.IsActive &&
                !string.IsNullOrWhiteSpace(user.Email) &&
                !string.IsNullOrWhiteSpace(user.PasswordHash) &&
                user.Role is not null &&
                user.Role.IsActive &&
                user.Role.RoleName.Contains("Coordinator", StringComparison.OrdinalIgnoreCase))
            .Select(user => new
            {
                user.UserId,
                user.EmployeeNo,
                user.FirstName,
                user.LastName,
                user.Email,
                user.RoleId,
                Role = user.Role.RoleName,
                user.IsActive
            })
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ToList();

        return Ok(coordinators);
    }

    // =========================================================
    // UPDATE REFERRAL TRACKER DETAILS
    // =========================================================

    [HttpPut("{referralId:guid}/tracker")]
    [Authorize(Policy = PermissionPolicies.ReferralsManage)]
    public async Task<IActionResult> UpdateTrackerDetails(
        Guid referralId,
        [FromBody] UpdateReferralTrackerDetailsCommand request,
        CancellationToken cancellationToken)
    {
        if (referralId != request.ReferralId)
            return BadRequest(new { message = "Referral identifier does not match the request." });

        try
        {
            await _sender.Send(request, cancellationToken);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

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
    CancellationToken cancellationToken)
    {
        if (pdf is null || pdf.Length <= 0)
            return BadRequest(new { message = "Please upload a non-empty referral PDF." });

        if (!pdf.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only PDF files are accepted." });

        if (pdf.Length > MaxFileSize)
            return BadRequest(new { message = "Referral PDF cannot exceed 10 MB." });

        try
        {
            await using Stream stream = pdf.OpenReadStream();

            var result = await _documentExtraction.ExtractAsync(
                stream,
                cancellationToken);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
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
        // Coordinator assignment is always tied to a real CCAP user account.
        // Non-administrators are restricted to their own account.
        if (!User.IsInRole("Administrator"))
        {
            var currentUserIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(currentUserIdValue, out var currentUserId))
            {
                return Unauthorized(new
                {
                    message = "Unable to determine the authenticated user."
                });
            }

            request.CoordinatorId = currentUserId;
        }

        if (request.CoordinatorId.HasValue)
        {
            var coordinator = await _users.GetByIdAsync(
                request.CoordinatorId.Value,
                cancellationToken);

            var isEligibleCoordinator =
                coordinator is not null &&
                coordinator.IsActive &&
                !string.IsNullOrWhiteSpace(coordinator.Email) &&
                !string.IsNullOrWhiteSpace(coordinator.PasswordHash) &&
                coordinator.Role is not null &&
                coordinator.Role.IsActive &&
                coordinator.Role.RoleName.Contains(
                    "Coordinator",
                    StringComparison.OrdinalIgnoreCase);

            if (!isEligibleCoordinator)
            {
                return BadRequest(new
                {
                    message = "Assigned CC must be an active Care Coordinator account with valid login credentials."
                });
            }

            request.TrackerCoordinatorName =
                $"{coordinator!.FirstName} {coordinator.LastName}".Trim();
        }
        else
        {
            request.TrackerCoordinatorName = null;
        }

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


            // =====================================================
            // SUCCESS
            // =====================================================

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
