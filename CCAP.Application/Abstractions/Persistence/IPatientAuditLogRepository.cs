using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IPatientAuditLogRepository
{
    Task AddAsync(PatientAuditLog log, CancellationToken cancellationToken);
    Task<List<PatientAuditLog>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken);
}
