using MediatR;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Admin.DTOs;

namespace CCAP.Application.Features.Admin.Queries.GetRolePermissions;

public sealed class GetRolePermissionsQueryHandler : IRequestHandler<GetRolePermissionsQuery, RoleDetailsDto>
{
    private readonly IRoleRepository _roles;

    public GetRolePermissionsQueryHandler(IRoleRepository roles) => _roles = roles;

    public async Task<RoleDetailsDto> Handle(
        GetRolePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var role = await _roles.GetByIdAsync(request.RoleId, cancellationToken)
            ?? throw new KeyNotFoundException("Role not found.");

        return new RoleDetailsDto(
            role.RoleId,
            role.RoleName,
            role.Description ?? string.Empty,
            role.IsActive,
            role.RolePermissions
                .Where(x => x.Permission is not null)
                .Select(x => new PermissionDto(
                    x.Permission.PermissionId,
                    x.Permission.PermissionCode,
                    x.Permission.PermissionName,
                    x.Permission.Module,
                    x.Permission.Description ?? string.Empty))
                .ToList());
    }
}
