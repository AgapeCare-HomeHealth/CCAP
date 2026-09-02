using CCAP.Application.Features.ReferralDrafts.DTOs;
using MediatR;

namespace CCAP.Application.Features.ReferralDrafts.Queries.GetReferralDraftById;

public sealed record GetReferralDraftByIdQuery(
    Guid ReferralDraftId)
    : IRequest<ReferralDraftDetailsDto?>;