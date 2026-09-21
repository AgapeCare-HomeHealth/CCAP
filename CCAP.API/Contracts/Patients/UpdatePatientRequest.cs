namespace CCAP.API.Contracts.Patients;

public sealed class UpdatePatientRequest
{
    public string MRN { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public DateOnly? SocDate { get; set; }
}
