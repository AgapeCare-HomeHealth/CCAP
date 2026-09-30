namespace CCAP.Web.Features.Authentication.Models;

public sealed class LoginResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public DateTime? Expiration { get; set; }
    public Guid? UserId { get; set; }
    public string? FullName { get; set; }
    public string? Role { get; set; }
}
