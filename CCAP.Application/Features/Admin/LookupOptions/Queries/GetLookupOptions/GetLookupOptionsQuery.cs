using MediatR;
namespace CCAP.Application.Features.Admin.LookupOptions.Queries.GetLookupOptions;
public sealed record GetLookupOptionsQuery(string? LookupType, bool ActiveOnly = false) : IRequest<List<LookupOptionDto>>;
