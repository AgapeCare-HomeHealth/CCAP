using MediatR;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Patients.DTOs;

namespace CCAP.Application.Features.Patients.Queries.GetServiceTypes;

public sealed class GetServiceTypesQueryHandler : IRequestHandler<GetServiceTypesQuery, List<ServiceTypeDto>>
{
    private readonly IServiceTypeRepository _repository;
    public GetServiceTypesQueryHandler(IServiceTypeRepository repository) => _repository = repository;

    public async Task<List<ServiceTypeDto>> Handle(GetServiceTypesQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetActiveAsync(cancellationToken);
        return items.Select(x => new ServiceTypeDto(
            x.ServiceTypeId, x.Code, x.Name, x.Icon, x.CssClass)).ToList();
    }
}
