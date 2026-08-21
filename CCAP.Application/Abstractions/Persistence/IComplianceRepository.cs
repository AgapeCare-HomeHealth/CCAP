using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IComplianceRepository
{
    Task AddAsync(
        ComplianceRecord record,
        CancellationToken cancellationToken);
}