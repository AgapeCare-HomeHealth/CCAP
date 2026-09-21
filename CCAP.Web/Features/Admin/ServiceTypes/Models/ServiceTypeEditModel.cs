using System.ComponentModel.DataAnnotations;
namespace CCAP.Web.Features.Admin.ServiceTypes.Models;

public sealed class ServiceTypeEditModel
{
    public Guid ServiceTypeId { get; set; }
    [Required, StringLength(50)] public string Code { get; set; } = string.Empty;
    [Required, StringLength(150)] public string Name { get; set; } = string.Empty;
    [StringLength(100)] public string Icon { get; set; } = string.Empty;
    [StringLength(100)] public string CssClass { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
