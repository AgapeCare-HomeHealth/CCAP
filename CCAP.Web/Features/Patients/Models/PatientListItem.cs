namespace CCAP.Web.Features.Patients.Models;

public class PatientListItem
{
    public Guid PatientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MRN { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PrimaryDiagnosis { get; set; } = string.Empty;
    public string AssignedClinician { get; set; } = string.Empty;
    public string NextVisit { get; set; } = string.Empty;
}
