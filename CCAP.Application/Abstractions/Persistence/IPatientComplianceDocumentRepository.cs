using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IPatientComplianceDocumentRepository
{
    Task AddAsync(
        PatientComplianceDocument document,
        CancellationToken cancellationToken);

    Task<PatientComplianceDocument?> GetLatestAsync(
        Guid patientId,
        string requirementCode,
        CancellationToken cancellationToken);
}
