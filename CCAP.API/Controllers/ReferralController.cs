using CCAP.API.Contracts.Referrals;
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
                    request.AuthorizationDate,
                    request.ApprovedVisits,
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

                    request.Pdf?.FileName ?? request.PdfFileName,

                    request.Pdf?.ContentType ?? request.PdfContentType,

                    request.Pdf?.Length ?? request.PdfSize
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
