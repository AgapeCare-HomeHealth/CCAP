using CCAP.Application.Features.Referrals.DTOs;
using MediatR;

namespace CCAP.Application.Features.Referrals.Queries.GetReferralCoordinatorFilterOptions;

public sealed record GetReferralCoordinatorFilterOptionsQuery
    : IRequest<IReadOnlyList<ReferralCoordinatorFilterOptionDto>>;
