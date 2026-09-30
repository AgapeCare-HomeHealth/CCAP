using MediatR;
namespace CCAP.Application.Features.Admin.ServiceTypes.Commands.DeleteServiceType;
public sealed record DeleteServiceTypeCommand(Guid ServiceTypeId) : IRequest;
