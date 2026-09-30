namespace CCAP.Web.Features.Admin.Users.Models;

public sealed class UserDto
{
    public Guid UserId { get; set; }
    public string EmployeeNo { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? MobileNo { get; set; }
    public Guid RoleId { get; set; }
    public Guid? DisciplineId { get; set; }
    public string Role { get; set; } = "";
    public string Discipline { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string FullName => $"{FirstName} {LastName}";
}
