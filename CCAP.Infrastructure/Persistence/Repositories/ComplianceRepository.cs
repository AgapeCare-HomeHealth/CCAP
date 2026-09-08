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
}