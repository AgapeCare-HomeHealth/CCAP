namespace CCAP.API.Contracts.Referrals;

public sealed class SaveReferralDraftRequest
{
    public Guid? ReferralDraftId { get; set; }
    public string Data { get; set; } = string.Empty;
}
