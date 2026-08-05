namespace CCAP.Web.Features.Admin.Users.Models
{
    public class UserStatisticsModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int Administrators { get; set; }
    }
}
