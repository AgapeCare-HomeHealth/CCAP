using Microsoft.AspNetCore.Http;

namespace CCAP.API.Contracts.Referrals;

public sealed class CreateReferralIntakeRequest
{
    public Guid? ReferralDraftId { get; set; }

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

    public DateOnly? AuthorizationDate { get; set; }

    public int? ApprovedVisits { get; set; }

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

    // File bytes are optional until cloud storage is enabled. These fields
    // preserve the uploaded file metadata in the meantime.
    public string? PdfFileName { get; set; }
    public string? PdfContentType { get; set; }
    public long? PdfSize { get; set; }
}
