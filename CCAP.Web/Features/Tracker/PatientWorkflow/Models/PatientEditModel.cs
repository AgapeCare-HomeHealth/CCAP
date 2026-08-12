namespace CCAP.Web.Features.Tracker.PatientWorkflow.Models;

public sealed class PatientEditModel
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MRN { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateOnly? SocDate { get; set; }
    public string Coordinator { get; set; } = string.Empty;
}
