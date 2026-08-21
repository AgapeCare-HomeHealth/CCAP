using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;

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
}