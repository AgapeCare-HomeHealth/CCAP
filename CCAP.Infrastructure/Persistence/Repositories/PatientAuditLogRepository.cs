using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class PatientAuditLogRepository : IPatientAuditLogRepository
{
    private readonly AppDbContext _context;
    public PatientAuditLogRepository(AppDbContext context) => _context = context;

    public Task AddAsync(PatientAuditLog log, CancellationToken cancellationToken) =>
        _context.PatientAuditLogs.AddAsync(log, cancellationToken).AsTask();

    public Task<List<PatientAuditLog>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken) =>
        _context.PatientAuditLogs
            .Include(x => x.PerformedByUser)
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.OccurredAt)
            .AsNoTracking()
            .Take(200)
            .ToListAsync(cancellationToken);
}
