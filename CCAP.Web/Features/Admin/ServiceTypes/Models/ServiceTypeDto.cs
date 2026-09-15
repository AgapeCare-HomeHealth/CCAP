namespace CCAP.Web.Features.Admin.ServiceTypes.Models;

public sealed class ServiceTypeDto
{
    public Guid ServiceTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
