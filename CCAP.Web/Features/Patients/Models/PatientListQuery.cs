namespace CCAP.Web.Features.Patients.Models;

public sealed class PatientListQuery
{
    public string Search { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? ClinicianId { get; set; }
    public string SortBy { get; set; } = "Name";
    public bool SortDescending { get; set; }
}
