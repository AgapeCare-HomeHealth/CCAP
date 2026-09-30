using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IPatientTaskRepository
{
    Task AddAsync(PatientTask task, CancellationToken cancellationToken);
    Task<PatientTask?> GetByIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task<PatientTask?> GetPendingByPatientAndTitleAsync(Guid patientId, string title, CancellationToken cancellationToken);
}