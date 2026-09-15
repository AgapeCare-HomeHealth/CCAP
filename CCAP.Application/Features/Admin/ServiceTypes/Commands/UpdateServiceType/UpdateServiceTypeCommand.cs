using MediatR;
namespace CCAP.Application.Features.Admin.ServiceTypes.Commands.UpdateServiceType;
public sealed record UpdateServiceTypeCommand(Guid ServiceTypeId, string Code, string Name, string Icon, string CssClass, bool IsActive) : IRequest;
