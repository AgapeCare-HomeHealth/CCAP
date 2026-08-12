using CCAP.Domain.Enums;

namespace CCAP.Domain.Entities;

public sealed class Referral
{
    private Referral() { }

    public Guid ReferralId { get; private set; }
    public string ReferralNumber { get; private set; } = string.Empty;
    public Guid? PatientId { get; private set; }
    public DateTime ReferralDate { get; private set; }
    public ReferralStatus Status { get; private set; }
    public string? Source { get; private set; }
    public string? Priority { get; private set; }
    public Guid? AssignedUserId { get; private set; }
    public DateTime? AssignedAt { get; private set; }

    public Patient? Patient { get; private set; }
    public ApplicationUser? AssignedUser { get; private set; }

    public Referral(string referralNumber, string? source, string? priority)
    {
        ReferralId = Guid.NewGuid();
        ReferralNumber = referralNumber.Trim();
        ReferralDate = DateTime.UtcNow;
        Source = source;
        Priority = priority;
        Status = ReferralStatus.Received;
    }

    public void Assign(Guid userId)
    {
        AssignedUserId = userId;
        AssignedAt = DateTime.UtcNow;
        Status = ReferralStatus.Assigned;
    }

    public void MarkUnderReview() => Status = ReferralStatus.UnderReview;

    public void ConvertToPatient(Guid patientId)
    {
        PatientId = patientId;
        Status = ReferralStatus.ConvertedToPatient;
    }
}
