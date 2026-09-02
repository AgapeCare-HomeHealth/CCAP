using CCAP.Domain.Enums;

namespace CCAP.Domain.Entities;

public sealed class ReferralDraft
{
    private ReferralDraft()
    {
    }

    public ReferralDraft(
        Guid createdByUserId,
        string data)
    {
        ReferralDraftId = Guid.NewGuid();

        CreatedByUserId =
            createdByUserId;

        Data =
            data;

        Status =
            ReferralStatus.Draft;

        CreatedAt =
            DateTime.UtcNow;

        UpdatedAt =
            DateTime.UtcNow;
    }

    public Guid ReferralDraftId { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public string Data { get; private set; }
        = string.Empty;

    public ReferralStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public void Update(
        string data)
    {
        Data =
            data;

        UpdatedAt =
            DateTime.UtcNow;
    }

    public void Submit()
    {
        if (Status != ReferralStatus.Draft)
        {
            throw new InvalidOperationException(
                "Only draft referrals can be submitted.");
        }

        Status =
            ReferralStatus.UnderReview;

        UpdatedAt =
            DateTime.UtcNow;
    }
}