using System.ComponentModel.DataAnnotations;

namespace CCAP.Web.Features.Tracker.PatientWorkflow.Models;

public sealed class PatientEditModel
{
    public Guid PatientId { get; set; }
    [Required, StringLength(50)] public string MRN { get; set; } = string.Empty;
    [Required, StringLength(100)] public string FirstName { get; set; } = string.Empty;
    [StringLength(100)] public string MiddleName { get; set; } = string.Empty;
    [Required, StringLength(100)] public string LastName { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateOnly? SocDate { get; set; }
    public string Coordinator { get; set; } = string.Empty;
}
