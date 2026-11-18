using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Referrals.DTOs;
using MediatR;

namespace CCAP.Application.Features.Referrals.Queries.GetReferralListForExport;

public sealed class GetReferralListForExportQueryHandler
    : IRequestHandler<GetReferralListForExportQuery, IReadOnlyList<ReferralListItemDto>>
{
    private readonly IReferralTrackerRepository _repository;

    public GetReferralListForExportQueryHandler(IReferralTrackerRepository repository) =>
        _repository = repository;

    public Task<IReadOnlyList<ReferralListItemDto>> Handle(
        GetReferralListForExportQuery request,
        CancellationToken cancellationToken) =>
        _repository.GetListAsync(
            request.Search,
            request.Status,
            request.Source,
            request.Insurance,
            request.AssignedCoordinatorId,
            cancellationToken);
}
