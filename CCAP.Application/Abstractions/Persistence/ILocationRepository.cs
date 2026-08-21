using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface ILocationRepository
{
    Task<Location?> GetDefaultAsync(
        CancellationToken cancellationToken);

    Task AddAsync(
        Location location,
        CancellationToken cancellationToken);
}