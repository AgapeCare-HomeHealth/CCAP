namespace CCAP.Application.Features.Referrals.DTOs;

public sealed class ReferralDocumentExtractionResult
{
    public bool OcrUsed { get; init; }
    public double? Confidence { get; init; }
    public int DetectedFieldCount { get; set; }
    public string? RawText { get; init; }
    public string? DetectedForm { get; init; }
    public string? DetectedFormVersion { get; init; }

    public string? MRN { get; init; }
    public string? FirstName { get; init; }
    public string? MiddleName { get; init; }
    public string? LastName { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? PrimaryPhone { get; init; }
    public string? AlternatePhone { get; init; }
    public string? StreetAddress { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public string? ZipCode { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactRelationship { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? ReferralNumber { get; init; }
    public DateTime? ReferralDate { get; init; }
    public string? ReferralSource { get; init; }
    public string? Priority { get; init; }
    public string? PrimaryInsurance { get; init; }
    public string? InsuranceMemberId { get; init; }
    public DateOnly? AuthorizationDate { get; init; }
    public int? ApprovedVisits { get; init; }
    public string? ReferringPhysician { get; init; }
    public string? PhysicianPhone { get; init; }
    public string? PrimaryDiagnosis { get; init; }
    public string? SecondaryDiagnosis { get; init; }
    public List<string> OrderedServices { get; init; } = [];
    public string? ReferralNotes { get; init; }
}
