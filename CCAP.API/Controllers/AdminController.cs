using CCAP.API.Contracts.Admin;
using CCAP.API.Authorization;
using CCAP.Application.Features.Admin.Commands.SetRolePermissions;
using CCAP.Application.Features.Admin.Commands.CreateRole;
using CCAP.Application.Features.Admin.Commands.UpdateRole;
using CCAP.Application.Features.Admin.Commands.DeleteRole;
using CCAP.Application.Features.Admin.Queries.GetDisciplines;
using CCAP.Application.Features.Admin.Queries.GetPermissions;
using CCAP.Application.Features.Admin.Queries.GetRolePermissions;
using CCAP.Application.Features.Admin.Queries.GetRoles;
using CCAP.Application.Features.Admin.LookupOptions.Commands.CreateLookupOption;
using CCAP.Application.Features.Admin.LookupOptions.Commands.UpdateLookupOption;
using CCAP.Application.Features.Admin.LookupOptions.Commands.DeleteLookupOption;
using CCAP.Application.Features.Admin.LookupOptions.Queries.GetLookupOptions;
using CCAP.Application.Features.Admin.ServiceTypes.Commands.CreateServiceType;
using CCAP.Application.Features.Admin.ServiceTypes.Commands.UpdateServiceType;
using CCAP.Application.Features.Admin.ServiceTypes.Commands.DeleteServiceType;
using CCAP.Application.Features.Admin.ServiceTypes.Queries.GetServiceTypes;
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
    [HttpGet("lookup-options")]
    [Authorize(Policy = PermissionPolicies.LookupOptionsView)]
    public async Task<IActionResult> GetLookupOptions([FromQuery] string? type, [FromQuery] bool activeOnly = false, CancellationToken cancellationToken = default) =>
        Ok(await _sender.Send(new GetLookupOptionsQuery(type, activeOnly), cancellationToken));

    [HttpPost("lookup-options")]
    [Authorize(Policy = PermissionPolicies.LookupOptionsManage)]
    public async Task<IActionResult> CreateLookupOption(CreateLookupOptionRequest request, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new CreateLookupOptionCommand(request.LookupType, request.Code, request.DisplayName, request.SortOrder, request.IsActive), cancellationToken));

    [HttpPut("lookup-options/{lookupOptionId:guid}")]
    [Authorize(Policy = PermissionPolicies.LookupOptionsManage)]
    public async Task<IActionResult> UpdateLookupOption(Guid lookupOptionId, UpdateLookupOptionRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new UpdateLookupOptionCommand(lookupOptionId, request.Code, request.DisplayName, request.SortOrder, request.IsActive), cancellationToken);
        return NoContent();
    }

    [HttpDelete("lookup-options/{lookupOptionId:guid}")]
    [Authorize(Policy = PermissionPolicies.LookupOptionsManage)]
    public async Task<IActionResult> DeleteLookupOption(Guid lookupOptionId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteLookupOptionCommand(lookupOptionId), cancellationToken);
        return NoContent();
    }

    [HttpGet("service-types")]
    [Authorize(Policy = PermissionPolicies.LookupOptionsView)]
    public async Task<IActionResult> GetServiceTypes([FromQuery] bool activeOnly = false, CancellationToken cancellationToken = default) =>
        Ok(await _sender.Send(new GetServiceTypesQuery(activeOnly), cancellationToken));

    [HttpPost("service-types")]
    [Authorize(Policy = PermissionPolicies.LookupOptionsManage)]
    public async Task<IActionResult> CreateServiceType(CreateServiceTypeRequest request, CancellationToken cancellationToken) =>
        Ok(await _sender.Send(new CreateServiceTypeCommand(request.Code, request.Name, request.Icon, request.CssClass, request.IsActive), cancellationToken));

    [HttpPut("service-types/{serviceTypeId:guid}")]
    [Authorize(Policy = PermissionPolicies.LookupOptionsManage)]
    public async Task<IActionResult> UpdateServiceType(Guid serviceTypeId, UpdateServiceTypeRequest request, CancellationToken cancellationToken)
    {
        await _sender.Send(new UpdateServiceTypeCommand(serviceTypeId, request.Code, request.Name, request.Icon, request.CssClass, request.IsActive), cancellationToken);
        return NoContent();
    }

    [HttpDelete("service-types/{serviceTypeId:guid}")]
    [Authorize(Policy = PermissionPolicies.LookupOptionsManage)]
    public async Task<IActionResult> DeleteServiceType(Guid serviceTypeId, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteServiceTypeCommand(serviceTypeId), cancellationToken);
        return NoContent();
    }

}

