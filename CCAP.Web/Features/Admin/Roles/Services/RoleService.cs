using CCAP.Web.Features.Admin.Roles.Models;
using CCAP.Web.Features.Authentication.Services;
using CCAP.Web.Features.MockData;

namespace CCAP.Web.Features.Admin.Roles.Services;

public sealed class RoleService
{
    private readonly CcapApiClient _api;
    private readonly MockDataStore _mock;
    private readonly MockDataOptions _options;

    public RoleService(CcapApiClient api, MockDataStore mock, MockDataOptions options)
    {
        _api = api;
        _mock = mock;
        _options = options;
    }

    public async Task<List<RoleApiDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
            return _mock.Roles.Select(Clone).ToList();
        return await _api.GetFromJsonAsync<List<RoleApiDto>>("api/admin/roles", cancellationToken) ?? [];
    }

    public async Task<List<PermissionDto>> GetPermissionsAsync(CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
            return _mock.Permissions.Select(Clone).ToList();
        return await _api.GetFromJsonAsync<List<PermissionDto>>("api/admin/permissions", cancellationToken) ?? [];
    }

    public async Task<RoleDetailsDto?> GetRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
        {
            var role = _mock.Roles.FirstOrDefault(x => x.RoleId == roleId);
            if (role is null) return null;
            var ids = _mock.RolePermissions.TryGetValue(roleId, out var permissionIds) ? permissionIds : [];
            return new RoleDetailsDto
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description,
                IsActive = role.IsActive,
                Permissions = _mock.Permissions.Where(x => ids.Contains(x.PermissionId)).Select(Clone).ToList()
            };
        }
        return await _api.GetFromJsonAsync<RoleDetailsDto>($"api/admin/roles/{roleId}", cancellationToken);
    }

    public async Task<RoleApiDto> CreateRoleAsync(RoleEditModel model, CancellationToken cancellationToken = default)
    {
        Validate(model);
        if (_options.Enabled)
        {
            if (_mock.Roles.Any(x => string.Equals(x.RoleName, model.RoleName.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("A role with this name already exists.");

            var role = new RoleApiDto
            {
                RoleId = Guid.NewGuid(),
                RoleName = model.RoleName.Trim(),
                Description = model.Description.Trim(),
                IsActive = model.IsActive,
                UserCount = 0,
                PermissionCount = 0
            };
            _mock.Roles.Add(role);
            _mock.RolePermissions[role.RoleId] = [];
            return Clone(role);
        }

        var response = await _api.PostAsJsonAsync("api/admin/roles", model, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<RoleApiDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("API returned no role.");
    }

    public async Task UpdateRoleAsync(RoleEditModel model, CancellationToken cancellationToken = default)
    {
        Validate(model);
        if (_options.Enabled)
        {
            var role = _mock.Roles.FirstOrDefault(x => x.RoleId == model.RoleId)
                ?? throw new KeyNotFoundException("Mock role not found.");

            if (_mock.Roles.Any(x => x.RoleId != model.RoleId && string.Equals(x.RoleName, model.RoleName.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("A role with this name already exists.");

            role.RoleName = model.RoleName.Trim();
            role.Description = model.Description.Trim();
            role.IsActive = model.IsActive;
            return;
        }

        var response = await _api.PutAsJsonAsync($"api/admin/roles/{model.RoleId}", model, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
        {
            var role = _mock.Roles.FirstOrDefault(x => x.RoleId == roleId)
                ?? throw new KeyNotFoundException("Mock role not found.");
            if (role.UserCount > 0)
                throw new InvalidOperationException("Cannot delete a role that still has assigned users.");

            _mock.Roles.Remove(role);
            _mock.RolePermissions.Remove(roleId);
            return;
        }

        var response = await _api.DeleteAsync($"api/admin/roles/{roleId}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task SetPermissionsAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
    {
        if (_options.Enabled)
        {
            var role = _mock.Roles.FirstOrDefault(x => x.RoleId == roleId)
                ?? throw new KeyNotFoundException("Mock role not found.");
            if (!role.IsActive)
                throw new InvalidOperationException("Cannot modify permissions for an inactive role.");

            _mock.RolePermissions[roleId] = permissionIds.ToHashSet();
            role.PermissionCount = _mock.RolePermissions[roleId].Count;
            return;
        }

        var response = await _api.PutAsJsonAsync($"api/admin/roles/{roleId}/permissions", new { PermissionIds = permissionIds.ToArray() }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static void Validate(RoleEditModel model)
    {
        if (string.IsNullOrWhiteSpace(model.RoleName))
            throw new ArgumentException("Role name is required.");
    }

    private static RoleApiDto Clone(RoleApiDto x) => new()
    {
        RoleId = x.RoleId,
        RoleName = x.RoleName,
        Description = x.Description,
        UserCount = x.UserCount,
        PermissionCount = x.PermissionCount,
        IsActive = x.IsActive
    };

    private static PermissionDto Clone(PermissionDto x) => new()
    {
        PermissionId = x.PermissionId,
        PermissionCode = x.PermissionCode,
        PermissionName = x.PermissionName,
        Module = x.Module,
        Description = x.Description
    };
}
