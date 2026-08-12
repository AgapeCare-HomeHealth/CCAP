namespace CCAP.Application.Features.Patients.DTOs;

public sealed record PatientListItemDto(
    Guid PatientId,
    string Name,
    string MRN,
    string Status,
    string PrimaryDiagnosis,
    string AssignedClinician,
    string? NextVisit);
