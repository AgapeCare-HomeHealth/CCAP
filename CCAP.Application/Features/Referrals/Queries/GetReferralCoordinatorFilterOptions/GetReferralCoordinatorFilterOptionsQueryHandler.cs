using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Referrals.DTOs;
using MediatR;

namespace CCAP.Application.Features.Referrals.Queries.GetReferralCoordinatorFilterOptions;

public sealed class GetReferralCoordinatorFilterOptionsQueryHandler
    : IRequestHandler<GetReferralCoordinatorFilterOptionsQuery, IReadOnlyList<ReferralCoordinatorFilterOptionDto>>
{
    private readonly IReferralTrackerRepository _repository;

    public GetReferralCoordinatorFilterOptionsQueryHandler(IReferralTrackerRepository repository) =>
        _repository = repository;

    public Task<IReadOnlyList<ReferralCoordinatorFilterOptionDto>> Handle(
        GetReferralCoordinatorFilterOptionsQuery request,
        CancellationToken cancellationToken) =>
        _repository.GetAssignedCoordinatorsAsync(cancellationToken);
}
