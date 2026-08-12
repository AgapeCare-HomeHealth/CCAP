using CCAP.API.Authorization;
using CCAP.Application.Features.Admin.Commands.SetRolePermissions;
using CCAP.Application.Features.Admin.Queries.GetDisciplines;
using CCAP.Application.Features.Admin.Queries.GetPermissions;
using CCAP.Application.Features.Admin.Queries.GetRolePermissions;
using CCAP.Application.Features.Admin.Queries.GetRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public sealed class AdminController : ControllerBase
{
    private readonly ISender _sender;
    public AdminController(ISender sender) => _sender = sender;

    [HttpGet("roles")]
    [Authorize(Policy = PermissionPolicies.RolesView)]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetRolesQuery(), cancellationToken));

    [HttpGet("roles/{roleId:guid}")]
    [Authorize(Policy = PermissionPolicies.RolesView)]
    public async Task<IActionResult> GetRole(
        Guid roleId,
        CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetRolePermissionsQuery(roleId), cancellationToken));

    [HttpGet("permissions")]
    [Authorize(Policy = PermissionPolicies.RolesView)]
    public async Task<IActionResult> GetPermissions(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetPermissionsQuery(), cancellationToken));

    [HttpPut("roles/{roleId:guid}/permissions")]
    [Authorize(Policy = PermissionPolicies.RolesManage)]
    public async Task<IActionResult> SetRolePermissions(
        Guid roleId,
        SetRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new SetRolePermissionsCommand(roleId, request.PermissionIds),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("disciplines")]
    [Authorize]
    public async Task<IActionResult> GetDisciplines(CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new GetDisciplinesQuery(), cancellationToken));
}

public sealed record SetRolePermissionsRequest(IReadOnlyCollection<Guid> PermissionIds);
