namespace CCAP.Web.Features.Admin.Roles.Models;

public sealed class RoleDetailsDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsActive { get; set; }
    public List<PermissionDto> Permissions { get; set; } = [];
}
