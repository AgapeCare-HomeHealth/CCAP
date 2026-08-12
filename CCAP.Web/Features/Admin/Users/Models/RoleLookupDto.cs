namespace CCAP.Web.Features.Admin.Users.Models;

public sealed class RoleLookupDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = "";
    public bool IsActive { get; set; }
}
