using System.ComponentModel.DataAnnotations;

namespace CCAP.Web.Features.Admin.Users.Models;

public sealed class UserEditModel
{
    public Guid UserId { get; set; }

    [Required]
    public string EmployeeNo { get; set; } = "";

    [Required]
    public string FirstName { get; set; } = "";

    [Required]
    public string LastName { get; set; } = "";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    public string? MobileNo { get; set; }

    [Required]
    public Guid RoleId { get; set; }

    public Guid? DisciplineId { get; set; }

    public string Password { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
