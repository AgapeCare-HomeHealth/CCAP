using CCAP.Application.Common.Models;
using CCAP.Application.Features.ReferralDrafts.DTOs;
using MediatR;

namespace CCAP.Application.Features.ReferralDrafts.Queries;

public sealed record GetReferralDraftsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? Search = null)
    : IRequest<PagedResult<ReferralDraftListDto>>;