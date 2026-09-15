using MediatR;
namespace CCAP.Application.Features.Admin.LookupOptions.Commands.DeleteLookupOption;
public sealed record DeleteLookupOptionCommand(Guid LookupOptionId) : IRequest;
