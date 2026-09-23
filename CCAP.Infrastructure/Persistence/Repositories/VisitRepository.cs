using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class VisitRepository : IVisitRepository
{
    private readonly AppDbContext _context;
    public VisitRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<Visit>> GetCalendarVisitsAsync(Guid userId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        => await _context.Visits.AsNoTracking().Include(x => x.Patient).Include(x => x.Clinician)
            .Where(x => x.AssignedUserId == userId && x.ScheduledDate >= startDate && x.ScheduledDate < endDate)
            .OrderBy(x => x.ScheduledDate).ToListAsync(cancellationToken);

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

    private static string NormalizeName(string value) =>
        string.Join(" ", value.Trim().ToUpperInvariant()
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    public async Task AddAsync(Visit visit, CancellationToken cancellationToken)
        => await _context.Visits.AddAsync(visit, cancellationToken);
}
