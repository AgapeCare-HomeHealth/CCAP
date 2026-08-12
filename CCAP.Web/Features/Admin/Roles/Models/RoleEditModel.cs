namespace CCAP.Web.Features.Admin.Roles.Models;

public sealed class RoleEditModel
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
