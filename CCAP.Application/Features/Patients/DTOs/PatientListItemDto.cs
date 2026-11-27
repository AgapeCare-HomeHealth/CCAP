namespace CCAP.Application.Features.Patients.DTOs;

public sealed record PatientListItemDto(
    Guid PatientId,
    string Name,
    string MRN,
    string Status,
    string Episode,
    string PrimaryDiagnosis,
    string AssignedClinician,
    string AssignedPrimaryCC,
    string? NextVisit,
    Guid? AssignedClinicianId,
    string MyAssignmentRole,
    DateOnly? SocDate,
    DateOnly? RocDate,
    DateOnly? RecertDate,
    DateOnly? TifDate);
