using MediatR;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Admin.DTOs;

namespace CCAP.Application.Features.Admin.Queries.GetDisciplines;

public sealed class GetDisciplinesQueryHandler : IRequestHandler<GetDisciplinesQuery, List<DisciplineDto>>
{
    private readonly IAdminLookupRepository _repository;
    public GetDisciplinesQueryHandler(IAdminLookupRepository repository) => _repository = repository;

    public async Task<List<DisciplineDto>> Handle(GetDisciplinesQuery request, CancellationToken cancellationToken)
    {
        var items = await _repository.GetDisciplinesAsync(cancellationToken);
        return items.Select(x => new DisciplineDto(x.DisciplineId, x.Code, x.Name)).ToList();
    }
}
