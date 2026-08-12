using System.Net.Http.Json;
using CCAP.Web.Features.Admin.Roles.Models;
using CCAP.Web.Features.Authentication.Services;

namespace CCAP.Web.Features.Admin.Roles.Services;

public sealed class RoleService
{
    private readonly CcapApiClient _api;

    public RoleService(CcapApiClient api)
    {
        _api = api;
    }

    public async Task<List<RoleApiDto>> GetRolesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _api.GetFromJsonAsync<List<RoleApiDto>>(
            "api/admin/roles",
            cancellationToken) ?? [];
    }

    public async Task<List<PermissionDto>> GetPermissionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _api.GetFromJsonAsync<List<PermissionDto>>(
            "api/admin/permissions",
            cancellationToken) ?? [];
    }

    public async Task<RoleDetailsDto?> GetRoleAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await _api.GetFromJsonAsync<RoleDetailsDto>(
            $"api/admin/roles/{roleId}",
            cancellationToken);
    }

    public async Task SetPermissionsAsync(
        Guid roleId,
        IEnumerable<Guid> permissionIds,
        CancellationToken cancellationToken = default)
    {
        var response = await _api.PutAsJsonAsync(
            $"api/admin/roles/{roleId}/permissions",
            new
            {
                PermissionIds = permissionIds.ToArray()
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}