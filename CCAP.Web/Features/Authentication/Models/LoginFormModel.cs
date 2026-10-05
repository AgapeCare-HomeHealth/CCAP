using System.ComponentModel.DataAnnotations;

namespace CCAP.Web.Features.Authentication.Models;

public sealed class LoginFormModel
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [StringLength(320, ErrorMessage = "Email cannot exceed 320 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [StringLength(
        200,
        MinimumLength = 8,
        ErrorMessage = "Password must be between 8 and 200 characters.")]
    public string Password { get; set; } = string.Empty;
}
