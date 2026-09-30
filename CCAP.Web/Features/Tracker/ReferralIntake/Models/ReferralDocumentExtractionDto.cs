namespace CCAP.Web.Features.Tracker.ReferralIntake.Models;

public sealed class ReferralDocumentExtractionDto
{
    public bool OcrUsed { get; set; }
    public string? DetectedForm { get; set; }
    public string? DetectedFormVersion { get; set; }
    public double? Confidence { get; set; }
    public int DetectedFieldCount { get; set; }
    public string? RawText { get; set; }

    public string? MRN { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? PrimaryPhone { get; set; }
    public string? AlternatePhone { get; set; }
    public string? StreetAddress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? ReferralNumber { get; set; }
    public DateTime? ReferralDate { get; set; }
    public string? ReferralSource { get; set; }
    public string? Priority { get; set; }
    public string? PrimaryInsurance { get; set; }
    public string? InsuranceMemberId { get; set; }
    public DateOnly? AuthorizationDate { get; set; }
    public int? ApprovedVisits { get; set; }
    public string? ReferringPhysician { get; set; }
    public string? PhysicianPhone { get; set; }
    public string? PrimaryDiagnosis { get; set; }
    public string? SecondaryDiagnosis { get; set; }
    public List<string> OrderedServices { get; set; } = [];
    public string? ReferralNotes { get; set; }
}
