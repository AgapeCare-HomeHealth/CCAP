namespace CCAP.Web.Features.Admin.Roles.Models
{
    public class RoleStatisticsModel
    {
        public int TotalRoles { get; set; }

        public int TotalPermissions { get; set; }

        public int AssignedUsers { get; set; }

        public int CustomRoles { get; set; }
    }
}
