using MediatR;

namespace CCAP.Application.Features.Admin.Commands.UpdateRole;

public sealed record UpdateRoleCommand(Guid RoleId, string RoleName, string? Description, bool IsActive) : IRequest;
