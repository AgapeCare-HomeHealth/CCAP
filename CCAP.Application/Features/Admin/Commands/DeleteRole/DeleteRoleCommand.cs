using MediatR;

namespace CCAP.Application.Features.Admin.Commands.DeleteRole;

public sealed record DeleteRoleCommand(Guid RoleId) : IRequest;
