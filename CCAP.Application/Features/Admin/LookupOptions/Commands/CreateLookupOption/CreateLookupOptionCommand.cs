using MediatR;

namespace CCAP.Application.Features.Admin.LookupOptions.Commands.CreateLookupOption;

public sealed record CreateLookupOptionCommand(string LookupType, string Code, string DisplayName, int SortOrder, bool IsActive) : IRequest<LookupOptionDto>;
