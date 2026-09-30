using MediatR;

namespace CCAP.Application.Features.Admin.Commands.SetRolePermissions;

public sealed record SetRolePermissionsCommand(
    Guid RoleId,
    IReadOnlyCollection<Guid> PermissionIds) : IRequest;
