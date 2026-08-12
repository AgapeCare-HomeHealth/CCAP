namespace CCAP.Web.Features.Tracker.PatientWorkflow.Models;

public sealed class ServiceTypeDto
{
    public Guid ServiceTypeId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public string CssClass { get; set; } = "";
}
