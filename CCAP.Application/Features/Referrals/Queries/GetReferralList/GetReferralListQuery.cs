using CCAP.Application.Common.Models;
using CCAP.Application.Features.Referrals.DTOs;
using MediatR;

namespace CCAP.Application.Features.Referrals.Queries.GetReferralList;

public sealed record GetReferralListQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    string? Status = null,
    string? Source = null,
    string? Insurance = null) : IRequest<PagedResult<ReferralListItemDto>>;
