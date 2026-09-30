using CCAP.Application.Abstractions.Persistence;
using MediatR;
namespace CCAP.Application.Features.Admin.LookupOptions.Queries.GetLookupOptions;
public sealed class GetLookupOptionsQueryHandler : IRequestHandler<GetLookupOptionsQuery,List<LookupOptionDto>>
{
    private readonly ILookupOptionRepository _repository; public GetLookupOptionsQueryHandler(ILookupOptionRepository repository)=>_repository=repository;
    public async Task<List<LookupOptionDto>> Handle(GetLookupOptionsQuery request,CancellationToken cancellationToken)
        => (await _repository.GetAsync(request.LookupType,request.ActiveOnly,cancellationToken)).Select(x=>new LookupOptionDto(x.LookupOptionId,x.LookupType,x.Code,x.DisplayName,x.SortOrder,x.IsActive)).ToList();
}
