using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IPatientTaskRepository
{
    Task AddAsync(
        PatientTask task,
        CancellationToken cancellationToken);
}