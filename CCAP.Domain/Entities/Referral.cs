using CCAP.Domain.Enums;

namespace CCAP.Domain.Entities;

public sealed class Referral
{
    private Referral()
    {
    }

    public Guid ReferralId { get; private set; }

    public string ReferralNumber { get; private set; }
        = string.Empty;

    public Guid? PatientId { get; private set; }

    public DateTime ReferralDate { get; private set; }

    public ReferralStatus Status { get; private set; }

    public string? Source { get; private set; }

    public string? Priority { get; private set; }

    public Guid? AssignedUserId { get; private set; }

    public DateTime? AssignedAt { get; private set; }

    public Guid LocationId { get; private set; }

    public Guid? DisciplineId { get; private set; }

    public string? VisitPriority { get; private set; }

    public string? CaseStatus { get; private set; }

    public string? PrimaryInsurance { get; private set; }

    public string? InsuranceMemberId { get; private set; }

    public bool AuthorizationRequired { get; private set; }

    public string? ReferringPhysician { get; private set; }

    public string? PhysicianPhone { get; private set; }

    public string? SecondaryDiagnosis { get; private set; }

    public string? ReferralNotes { get; private set; }

    public string? InternalNotes { get; private set; }

    public Patient? Patient { get; private set; }

    public ApplicationUser? AssignedUser { get; private set; }

    public Location Location { get; private set; } = null!;

    public Discipline? Discipline { get; private set; }

    public ICollection<ReferralDocument> Documents { get; private set; }
        = new List<ReferralDocument>();

    public Referral(
        string referralNumber,
        DateTime referralDate,
        string? source,
        string? priority,
        Guid locationId,
        Guid? disciplineId,
        string? visitPriority,
        string? caseStatus,
        string? primaryInsurance,
        string? insuranceMemberId,
        bool authorizationRequired,
        string? referringPhysician,
        string? physicianPhone,
        string? secondaryDiagnosis,
        string? referralNotes,
        string? internalNotes)
    {
        if (string.IsNullOrWhiteSpace(referralNumber))
            throw new ArgumentException(
                "Referral number is required.",
                nameof(referralNumber));

        if (locationId == Guid.Empty)
            throw new ArgumentException(
                "Location is required.",
                nameof(locationId));

        ReferralId = Guid.NewGuid();

        ReferralNumber =
            referralNumber.Trim();

        ReferralDate =
            referralDate;

        Source =
            Normalize(source);

        Priority =
            Normalize(priority);

        LocationId =
            locationId;

        DisciplineId =
            disciplineId;

        VisitPriority =
            Normalize(visitPriority);

        CaseStatus =
            Normalize(caseStatus);

        PrimaryInsurance =
            Normalize(primaryInsurance);

        InsuranceMemberId =
            Normalize(insuranceMemberId);

        AuthorizationRequired =
            authorizationRequired;

        ReferringPhysician =
            Normalize(referringPhysician);

        PhysicianPhone =
            Normalize(physicianPhone);

        SecondaryDiagnosis =
            Normalize(secondaryDiagnosis);

        ReferralNotes =
            Normalize(referralNotes);

        InternalNotes =
            Normalize(internalNotes);

        Status =
            ReferralStatus.Received;
    }

    public void Assign(Guid userId)
    {
        AssignedUserId = userId;

        AssignedAt = DateTime.UtcNow;

        Status = ReferralStatus.Assigned;
    }

    public void MarkUnderReview()
        => Status = ReferralStatus.UnderReview;

    public void ConvertToPatient(Guid patientId)
    {
        PatientId = patientId;

        Status = ReferralStatus.ConvertedToPatient;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}