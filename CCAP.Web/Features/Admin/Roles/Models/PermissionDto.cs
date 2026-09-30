namespace CCAP.Web.Features.Admin.Roles.Models;

public sealed class PermissionDto
{
    public Guid PermissionId { get; set; }
    public string PermissionCode { get; set; } = "";
    public string PermissionName { get; set; } = "";
    public string Module { get; set; } = "";
    public string Description { get; set; } = "";
}
