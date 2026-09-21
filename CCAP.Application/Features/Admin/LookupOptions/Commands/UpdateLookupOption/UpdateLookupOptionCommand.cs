using MediatR;

namespace CCAP.Application.Features.Admin.LookupOptions.Commands.UpdateLookupOption;

public sealed record UpdateLookupOptionCommand(Guid LookupOptionId, string Code, string DisplayName, int SortOrder, bool IsActive) : IRequest;
