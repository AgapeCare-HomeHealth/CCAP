using MediatR;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Admin.DTOs;

namespace CCAP.Application.Features.Admin.Queries.GetPermissions;

public sealed class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, List<PermissionDto>>
{
    private readonly IRoleRepository _roles;
    public GetPermissionsQueryHandler(IRoleRepository roles) => _roles = roles;

    public async Task<List<PermissionDto>> Handle(
        GetPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await _roles.GetPermissionsAsync(cancellationToken);
        return permissions.Select(x => new PermissionDto(
            x.PermissionId,
            x.PermissionCode,
            x.PermissionName,
            x.Module,
            x.Description ?? string.Empty)).ToList();
    }
}
