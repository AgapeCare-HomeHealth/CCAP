using CCAP.API.Authorization;
using CCAP.Application.Features.Admin.Commands.SetRolePermissions;
using CCAP.Application.Features.Admin.Commands.CreateRole;
using CCAP.Application.Features.Admin.Commands.UpdateRole;
using CCAP.Application.Features.Admin.Commands.DeleteRole;
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

    [HttpPost("roles")]
    [Authorize(Policy = PermissionPolicies.RolesManage)]
    public async Task<IActionResult> CreateRole(
        CreateRoleRequest request,
        CancellationToken cancellationToken) =>
        Ok(await _sender.Send(
            new CreateRoleCommand(request.RoleName, request.Description, request.IsActive),
            cancellationToken));

    [HttpPut("roles/{roleId:guid}")]
    [Authorize(Policy = PermissionPolicies.RolesManage)]
    public async Task<IActionResult> UpdateRole(
        Guid roleId,
        UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new UpdateRoleCommand(roleId, request.RoleName, request.Description, request.IsActive),
            cancellationToken);
        return NoContent();
    }

    [HttpDelete("roles/{roleId:guid}")]
    [Authorize(Policy = PermissionPolicies.RolesManage)]
    public async Task<IActionResult> DeleteRole(
        Guid roleId,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteRoleCommand(roleId), cancellationToken);
        return NoContent();
    }

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
public sealed record CreateRoleRequest(string RoleName, string? Description, bool IsActive);
public sealed record UpdateRoleRequest(string RoleName, string? Description, bool IsActive);
