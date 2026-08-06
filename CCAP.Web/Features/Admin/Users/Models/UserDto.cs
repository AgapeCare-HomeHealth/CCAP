namespace CCAP.Web.Features.Admin.Users.Models
{
    public class UserDto
    {
        public Guid UserId { get; set; }

        public string EmployeeNo { get; set; } = "";

        public string FirstName { get; set; } = "";

        public string LastName { get; set; } = "";

        public string Email { get; set; } = "";

        public string Role { get; set; } = "";

        public string Discipline { get; set; } = "";

        public bool IsActive { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }
}
