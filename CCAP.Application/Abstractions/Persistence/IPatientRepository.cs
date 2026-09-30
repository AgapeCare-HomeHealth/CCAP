using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IPatientRepository
{
    Task<Patient?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Patient?> GetByIdForUpdateAsync(
        Guid patientId,
        CancellationToken cancellationToken);

    Task<Patient?> GetByIdForWorkflowUpdateAsync(
        Guid patientId,
        CancellationToken cancellationToken);

    Task<Patient?> GetByMrnAsync(
        string mrn,
        CancellationToken cancellationToken);

    Task<List<Patient>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<(IReadOnlyList<Patient> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        string? status,
        Guid? clinicianId,
        string sortBy,
        bool sortDescending,
        CancellationToken cancellationToken);

    Task AddAsync(
        Patient patient,
        CancellationToken cancellationToken);

    void Update(Patient patient);
}