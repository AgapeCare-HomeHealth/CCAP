namespace CCAP.Domain.Entities;

public sealed class Role
{
    private Role() { }

    public Role(string roleName, string? description = null)
    {
        RoleId = Guid.NewGuid();
        RoleName = roleName.Trim();
        Description = description?.Trim();
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid RoleId { get; private set; }
    public string RoleName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<ApplicationUser> Users { get; private set; } = new List<ApplicationUser>();
    public ICollection<RolePermission> RolePermissions { get; private set; } = new List<RolePermission>();

    public void Activate() { IsActive = true; UpdatedAt = DateTime.UtcNow; }
    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }
    public void Update(string roleName, string? description)
    {
        RoleName = roleName.Trim();
        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
