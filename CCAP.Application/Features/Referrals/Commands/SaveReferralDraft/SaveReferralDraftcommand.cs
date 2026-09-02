using MediatR;

namespace CCAP.Application.Features.Referrals.Commands.SaveReferralDraft;

public sealed record SaveReferralDraftCommand(
    Guid? ReferralDraftId,
    Guid CreatedByUserId,
    string Data)
    : IRequest<SaveReferralDraftResult>;