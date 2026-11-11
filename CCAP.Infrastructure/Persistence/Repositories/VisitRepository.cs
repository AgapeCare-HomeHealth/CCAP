using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class VisitRepository : IVisitRepository
{
    private readonly AppDbContext _context;
    public VisitRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Visit>> GetCalendarVisitsAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        bool canViewAllSchedules,
        bool isCareCoordinator,
        CancellationToken cancellationToken)
    {
        var query = _context.Visits.AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.Clinician)
            .Include(x => x.CareCoordinator)
            .Where(x => x.ScheduledDate >= startDate && x.ScheduledDate < endDate);

        if (!canViewAllSchedules)
        {
            query = isCareCoordinator
                ? query.Where(x => x.CareCoordinatorId == userId ||
                    (!x.CareCoordinatorId.HasValue && x.AssignedUserId == userId))
                : query.Where(x => x.AssignedUserId == userId);
        }

        return await query.OrderBy(x => x.ScheduledDate).ToListAsync(cancellationToken);
    }

    public Task<List<Visit>> GetForAssignmentAsync(Guid userId, IReadOnlyCollection<Guid> visitIds, CancellationToken cancellationToken)
        => _context.Visits.Where(x => x.AssignedUserId == userId && visitIds.Contains(x.VisitId)).ToListAsync(cancellationToken);

    public Task<Visit?> GetByIdForUpdateAsync(Guid visitId, CancellationToken cancellationToken)
        => _context.Visits.FirstOrDefaultAsync(x => x.VisitId == visitId, cancellationToken);

    public Task<Visit?> FindDuplicateAsync(Guid userId, Guid patientId, DateTime scheduledDate, CancellationToken cancellationToken)
        => _context.Visits.FirstOrDefaultAsync(
            x => x.AssignedUserId == userId && x.PatientId == patientId && x.ScheduledDate == scheduledDate,
            cancellationToken);

    public Task<Visit?> FindImportedDuplicateAsync(Guid userId, string patientName, DateTime scheduledDate, CancellationToken cancellationToken)
    {
        var normalizedName = NormalizeName(patientName);

        return _context.Visits.FirstOrDefaultAsync(
            x => x.AssignedUserId == userId &&
                 x.PatientId == null &&
                 x.ScheduledDate == scheduledDate &&
                 x.PatientName.ToUpper() == normalizedName,
            cancellationToken);
    }

    public Task<Visit?> FindScheduleDuplicateAsync(
        Guid userId,
        string patientName,
        DateTime scheduledDate,
        string timeBlock,
        string visitType,
        CancellationToken cancellationToken)
    {
        var normalizedName = NormalizeName(patientName);
        var normalizedTimeBlock = timeBlock.Trim().ToUpperInvariant();
        var normalizedVisitType = visitType.Trim().ToUpperInvariant();

        return _context.Visits.FirstOrDefaultAsync(
            x => (x.AssignedUserId == userId || x.CareCoordinatorId == userId) &&
                 x.ScheduledDate.Date == scheduledDate.Date &&
                 x.PatientName.ToUpper() == normalizedName &&
                 x.TimeBlock != null &&
                 x.TimeBlock.ToUpper() == normalizedTimeBlock &&
                 x.VisitType.ToUpper() == normalizedVisitType,
            cancellationToken);
    }

    private static string NormalizeName(string value) =>
        string.Join(" ", value.Trim().ToUpperInvariant()
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public void RemoveRange(IEnumerable<Visit> visits) => _context.Visits.RemoveRange(visits);

    public async Task AddAsync(Visit visit, CancellationToken cancellationToken)
        => await _context.Visits.AddAsync(visit, cancellationToken);
}
