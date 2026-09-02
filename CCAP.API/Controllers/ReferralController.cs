using CCAP.API.Authorization;
using CCAP.Application.Features.ReferralDrafts.Queries;
using CCAP.Application.Features.Referrals.Commands.CreateReferralIntake;
using CCAP.Application.Features.Referrals.Commands.SaveReferralDraft;
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

    private const long MaxFileSize =
        10 * 1024 * 1024; // 10 MB

    public ReferralController(ISender sender)
    {
        _sender = sender;
    }

    // =========================================================
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
    // GET REFERRAL DRAFTS
    // ========================================================

    [HttpGet("drafts")]
    [Authorize(
    Policy = PermissionPolicies.ReferralsView)]
    public async Task<IActionResult> GetDrafts(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? search = null,
    CancellationToken cancellationToken = default)
    {
        var result =
            await _sender.Send(
                new GetReferralDraftsQuery(
                    pageNumber,
                    pageSize,
                    search),
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
                    request.MRN,
                    request.FirstName,
                    request.MiddleName,
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

                    request.ReferralNumber,
                    request.ReferralDate,
                    request.ReferralSource,
                    request.Priority,

                    request.PrimaryInsurance,
                    request.InsuranceMemberId,
                    request.AuthorizationRequired,

                    request.ReferringPhysician,
                    request.PhysicianPhone,

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

                    request.Pdf?.FileName,

                    request.Pdf?.ContentType
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


// =============================================================
// SAVE REFERRAL DRAFT INTAKE REQUEST
// =============================================================
public sealed class SaveReferralDraftRequest
{
    public Guid? ReferralDraftId { get; set; }

    public string Data { get; set; }
        = string.Empty;
}


// =============================================================
// CREATE REFERRAL INTAKE REQUEST
// =============================================================

public sealed class CreateReferralIntakeRequest
{
    // =========================================================
    // PATIENT
    // =========================================================

    public string MRN { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? PrimaryPhone { get; set; }

    public string? AlternatePhone { get; set; }

    public string? StreetAddress { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    // =========================================================
    // EMERGENCY CONTACT
    // =========================================================

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactRelationship { get; set; }

    public string? EmergencyContactPhone { get; set; }

    // =========================================================
    // REFERRAL
    // =========================================================

    public string ReferralNumber { get; set; }
        = string.Empty;

    public DateTime ReferralDate { get; set; }

    public string? ReferralSource { get; set; }

    public string? Priority { get; set; }

    // =========================================================
    // INSURANCE
    // =========================================================

    public string? PrimaryInsurance { get; set; }

    public string? InsuranceMemberId { get; set; }

    public bool AuthorizationRequired { get; set; }

    // =========================================================
    // PHYSICIAN
    // =========================================================

    public string? ReferringPhysician { get; set; }

    public string? PhysicianPhone { get; set; }

    // =========================================================
    // CLINICAL
    // =========================================================

    public string? PrimaryDiagnosis { get; set; }

    public string? SecondaryDiagnosis { get; set; }

    public List<string>? OrderedServices { get; set; }

    public string? ReferralNotes { get; set; }

    // =========================================================
    // ASSIGNMENT
    // =========================================================

    public Guid? CoordinatorId { get; set; }

    public Guid? ClinicianId { get; set; }

    public Guid? DisciplineId { get; set; }

    // =========================================================
    // SCHEDULING
    // =========================================================

    public DateOnly? SocDate { get; set; }

    public string? VisitPriority { get; set; }

    public string? CaseStatus { get; set; }

    // =========================================================
    // INTERNAL
    // =========================================================

    public string? InternalNotes { get; set; }

    // =========================================================
    // PDF
    // =========================================================

    public IFormFile? Pdf { get; set; }
}