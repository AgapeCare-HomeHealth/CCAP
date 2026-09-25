using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IVisitRepository
{
    Task<IReadOnlyList<Visit>> GetCalendarVisitsAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
    Task<Visit?> GetByIdForUpdateAsync(Guid visitId, CancellationToken cancellationToken);
    Task<Visit?> FindDuplicateAsync(Guid userId, Guid patientId, DateTime scheduledDate, CancellationToken cancellationToken);
    Task<Visit?> FindImportedDuplicateAsync(Guid userId, string patientName, DateTime scheduledDate, CancellationToken cancellationToken);
    Task<Visit?> FindScheduleDuplicateAsync(Guid userId, string patientName, DateTime scheduledDate, string timeBlock, CancellationToken cancellationToken);
    Task AddAsync(Visit visit, CancellationToken cancellationToken);
}
