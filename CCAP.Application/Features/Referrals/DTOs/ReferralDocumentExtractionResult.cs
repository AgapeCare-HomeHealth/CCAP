namespace CCAP.Application.Features.Referrals.DTOs;

public sealed class ReferralDocumentExtractionResult
{
    public bool OcrUsed { get; init; }
    public double? Confidence { get; init; }
    public int DetectedFieldCount { get; set; }
    public string? RawText { get; init; }
    public string? DetectedForm { get; init; }
    public string? DetectedFormVersion { get; init; }

    public string? MRN { get; set; }
    public bool MrnRequiresReview { get; set; }
    public List<ReferralMrnCandidate> MrnCandidates { get; set; } = [];
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
    public string? PrimaryInsurance { get; set; }
    public string? InsuranceMemberId { get; init; }
    public DateOnly? AuthorizationDate { get; init; }
    public int? ApprovedVisits { get; init; }
    public string? ReferringPhysician { get; set; }
    public string? PhysicianPhone { get; init; }
    public string? PrimaryDiagnosis { get; init; }
    public string? SecondaryDiagnosis { get; init; }
    public List<string> OrderedServices { get; init; } = [];
    public string? ReferralNotes { get; init; }
}

/// <summary>MRN candidates are backed by an explicit label in the PDF, not guessed from a nearby number.</summary>
public sealed class ReferralMrnCandidate
{
    public string Value { get; init; } = string.Empty;
    public int PageNumber { get; init; }
    public string Source { get; init; } = string.Empty;
}
