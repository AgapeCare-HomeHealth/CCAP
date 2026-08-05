namespace CCAP.Web.Features.Admin.Users.Models
{

    public class UserFilterModel
    {
        public string Search { get; set; } = string.Empty;

        public Guid? RoleId { get; set; }

        public Guid? DisciplineId { get; set; }

        public bool? IsActive { get; set; }
    }
}
