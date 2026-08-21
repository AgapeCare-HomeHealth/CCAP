using CCAP.API.Authorization;
using CCAP.Application.Features.Referrals.Commands.CreateReferralIntake;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        // =====================================================
        // VALIDATE PDF
        // =====================================================

        if (request.Pdf is null)
        {
            return BadRequest(new
            {
                message = "Referral PDF is required."
            });
        }

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
                message = "Referral PDF cannot exceed 10 MB."
            });
        }

        // Validate PDF content type
        if (!string.Equals(
                request.Pdf.ContentType,
                "application/pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Only PDF files are allowed."
            });
        }

        // Validate file extension
        if (!request.Pdf.FileName.EndsWith(
                ".pdf",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Only PDF files are allowed."
            });
        }

        // =====================================================
        // OPEN PDF STREAM
        // =====================================================

        await using var stream =
            request.Pdf.OpenReadStream();

        try
        {
            // =================================================
            // SEND APPLICATION COMMAND
            // =================================================

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

                    // PDF
                    stream,
                    request.Pdf.FileName,
                    request.Pdf.ContentType
                ),
                cancellationToken);

            // =================================================
            // SUCCESS
            // =================================================

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
    }
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