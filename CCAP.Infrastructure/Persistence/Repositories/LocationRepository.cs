using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class LocationRepository : ILocationRepository
{
    private readonly AppDbContext _context;

    public LocationRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Location?> GetDefaultAsync(
        CancellationToken cancellationToken) =>
        _context.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.IsActive && x.IsDefault,
                cancellationToken);

    public Task AddAsync(
        Location location,
        CancellationToken cancellationToken) =>
        _context.Locations
            .AddAsync(location, cancellationToken)
            .AsTask();
}