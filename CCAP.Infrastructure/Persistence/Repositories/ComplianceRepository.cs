using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class ComplianceRepository
    : IComplianceRepository
{
    private readonly AppDbContext _context;

    public ComplianceRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(
        ComplianceRecord record,
        CancellationToken cancellationToken) =>
        _context.ComplianceRecords
            .AddAsync(
                record,
                cancellationToken)
            .AsTask();

    public async Task<ComplianceRecord?> GetByPatientAndRequirementAsync(
        Guid patientId,
        string requirementCode,
        CancellationToken cancellationToken)
    {
        return await _context.ComplianceRecords
            .FirstOrDefaultAsync(
                x =>
                    x.PatientId == patientId &&
                    x.RequirementCode == requirementCode,
                cancellationToken);
    }

    public async Task<List<ComplianceRecord>> GetByPatientAsync(Guid patientId, CancellationToken cancellationToken)
    {
        // Include newly Added records in the current unit of work as well as persisted rows.
        // EF database queries do not return Added entities until SaveChanges is called.
        var persisted = await _context.ComplianceRecords
            .Where(x => x.PatientId == patientId)
            .ToListAsync(cancellationToken);

        var tracked = _context.ComplianceRecords.Local
            .Where(x => x.PatientId == patientId);

        return persisted
            .Concat(tracked)
            .DistinctBy(x => x.ComplianceRecordId)
            .ToList();
    }

}