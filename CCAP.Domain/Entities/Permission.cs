namespace CCAP.Domain.Entities;

public sealed class Permission
{
    private Permission() { }

    public Permission(string permissionCode, string permissionName, string module, string? description = null)
    {
        PermissionId = Guid.NewGuid();
        PermissionCode = permissionCode.Trim();
        PermissionName = permissionName.Trim();
        Module = module.Trim();
        Description = description?.Trim();
    }

    public Guid PermissionId { get; private set; }
    public string PermissionCode { get; private set; } = string.Empty;
    public string PermissionName { get; private set; } = string.Empty;
    public string Module { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();
}
