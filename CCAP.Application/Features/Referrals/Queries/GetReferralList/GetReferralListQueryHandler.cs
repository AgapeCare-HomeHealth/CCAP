using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Common.Models;
using CCAP.Application.Features.Referrals.DTOs;
using MediatR;

namespace CCAP.Application.Features.Referrals.Queries.GetReferralList;

public sealed class GetReferralListQueryHandler : IRequestHandler<GetReferralListQuery, PagedResult<ReferralListItemDto>>
{
    private readonly IReferralTrackerRepository _repository;
    public GetReferralListQueryHandler(IReferralTrackerRepository repository) => _repository = repository;

    public async Task<PagedResult<ReferralListItemDto>> Handle(GetReferralListQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.PageNumber);
        var size = Math.Clamp(request.PageSize, 1, 100);
        var (items, count) = await _repository.GetPagedListAsync(
            page, size, request.Search, request.Status, request.Source, request.Insurance, cancellationToken);
        return new PagedResult<ReferralListItemDto> { Items = items, PageNumber = page, PageSize = size, TotalCount = count };
    }
}
