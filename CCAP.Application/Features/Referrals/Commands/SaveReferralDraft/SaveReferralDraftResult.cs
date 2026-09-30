namespace CCAP.Application.Features.Referrals.Commands.SaveReferralDraft;

public sealed record SaveReferralDraftResult(
    Guid ReferralDraftId,
    string Status);