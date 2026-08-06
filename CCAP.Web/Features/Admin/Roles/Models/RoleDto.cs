namespace CCAP.Web.Features.Admin.Roles.Models
{
    public class RoleDto
    {
        public Guid RoleId { get; set; }

        public string RoleName { get; set; } = "";

        public string Description { get; set; } = "";

        public int UserCount { get; set; }

        public int PermissionCount { get; set; }

        public bool IsActive { get; set; }
    }
}
