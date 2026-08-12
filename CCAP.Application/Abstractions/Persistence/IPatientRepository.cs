using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Patient>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Patient patient, CancellationToken cancellationToken);
    void Update(Patient patient);
}
