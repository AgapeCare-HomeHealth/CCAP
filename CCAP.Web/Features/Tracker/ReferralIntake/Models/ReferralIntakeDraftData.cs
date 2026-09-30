namespace CCAP.Web.Features.Tracker.ReferralIntake.Models;

/// <summary>
/// Serializable referral draft state. Uploaded PDF bytes are intentionally
/// excluded; the file metadata is retained and the structured values extracted
/// from the PDF are preserved so a draft can be resumed without re-entry.
/// </summary>
public sealed class ReferralIntakeDraftData
{
    public string? ReferralPdfFileName { get; set; }
    public string? ReferralPdfContentType { get; set; }
    public long? ReferralPdfSize { get; set; }

    public string MRN { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string PrimaryPhone { get; set; } = string.Empty;
    public string AlternatePhone { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;

    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactRelationship { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;

    public string ReferralNumber { get; set; } = string.Empty;
    public DateTime ReferralDate { get; set; } = DateTime.Today;
    public string ReferralSource { get; set; } = string.Empty;
    public string Priority { get; set; } = "Routine";

    public string PrimaryInsurance { get; set; } = string.Empty;
    public string InsuranceMemberId { get; set; } = string.Empty;
    public DateOnly? AuthorizationDate { get; set; }
    public int? ApprovedVisits { get; set; }
    public bool AuthorizationRequired { get; set; }

    public string ReferringPhysician { get; set; } = string.Empty;
    public string PhysicianPhone { get; set; } = string.Empty;

    public string PrimaryDiagnosis { get; set; } = string.Empty;
    public string SecondaryDiagnosis { get; set; } = string.Empty;
    public List<string> OrderedServices { get; set; } = [];
    public string ReferralNotes { get; set; } = string.Empty;

    public Guid? CoordinatorId { get; set; }
    public Guid? ClinicianId { get; set; }
    public Guid? DisciplineId { get; set; }
    public DateOnly? SocDate { get; set; }
    public string VisitPriority { get; set; } = string.Empty;
    public string CaseStatus { get; set; } = string.Empty;
    public string InternalNotes { get; set; } = string.Empty;
}
