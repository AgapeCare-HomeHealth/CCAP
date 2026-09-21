using CCAP.Application.Abstractions.Persistence;
using MediatR;
using CCAP.Application.Features.Admin.ServiceTypes;
namespace CCAP.Application.Features.Admin.ServiceTypes.Queries.GetServiceTypes;
public sealed class GetServiceTypesQueryHandler : IRequestHandler<GetServiceTypesQuery, List<ServiceTypeAdminDto>>
{
    private readonly IServiceTypeRepository _repository;
    public GetServiceTypesQueryHandler(IServiceTypeRepository repository) => _repository = repository;
    public async Task<List<ServiceTypeAdminDto>> Handle(GetServiceTypesQuery request, CancellationToken cancellationToken)
    {
        var items = request.ActiveOnly ? await _repository.GetActiveAsync(cancellationToken) : await _repository.GetAllAsync(cancellationToken);
        return items.Select(x => new ServiceTypeAdminDto(x.ServiceTypeId, x.Code, x.Name, x.Icon, x.CssClass, x.IsActive)).ToList();
    }
}
