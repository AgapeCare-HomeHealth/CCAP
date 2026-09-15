using MediatR;
using CCAP.Application.Features.Admin.ServiceTypes;
namespace CCAP.Application.Features.Admin.ServiceTypes.Commands.CreateServiceType;
public sealed record CreateServiceTypeCommand(string Code, string Name, string Icon, string CssClass, bool IsActive) : IRequest<ServiceTypeAdminDto>;
