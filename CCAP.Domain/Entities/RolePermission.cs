namespace CCAP.Domain.Entities;

public sealed class RolePermission
{
    private RolePermission() { }

    public RolePermission(Guid roleId, Guid permissionId)
    {
        RolePermissionId = Guid.NewGuid();
        RoleId = roleId;
        PermissionId = permissionId;
    }

    public Guid RolePermissionId { get; private set; }
    public Guid RoleId { get; private set; }
    public Guid PermissionId { get; private set; }

    public Role Role { get; private set; } = null!;
    public Permission Permission { get; private set; } = null!;
}
