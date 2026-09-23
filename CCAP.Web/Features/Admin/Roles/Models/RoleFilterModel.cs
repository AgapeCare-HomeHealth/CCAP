namespace CCAP.Web.Features.Admin.Roles.Models
{
    public class RoleFilterModel
    {
        public string Search { get; set; } = "";

        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "RoleName";
        public bool SortDescending { get; set; }
    }
}
