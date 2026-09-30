using System.ComponentModel.DataAnnotations;

namespace CCAP.Web.Features.Admin.Roles.Models;

public sealed class RoleEditModel
{
    public Guid RoleId { get; set; }
    [Required, StringLength(100)] public string RoleName { get; set; } = string.Empty;
    [StringLength(500)] public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
