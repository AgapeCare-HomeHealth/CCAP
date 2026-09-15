using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class LookupOptionRepository : ILookupOptionRepository
{
    private readonly AppDbContext _context;
    public LookupOptionRepository(AppDbContext context) => _context = context;

    public Task<string?> GetCanonicalLookupTypeAsync(string lookupType, CancellationToken cancellationToken) =>
        _context.LookupOptions
            .Where(x => x.LookupType.ToUpper() == lookupType.Trim().ToUpper())
            .Select(x => x.LookupType)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<List<LookupOption>> GetAsync(string? lookupType, bool activeOnly, CancellationToken cancellationToken) =>
        _context.LookupOptions
            .Where(x => string.IsNullOrWhiteSpace(lookupType) || x.LookupType == lookupType)
            .Where(x => !activeOnly || x.IsActive)
            .AsNoTracking()
            .OrderBy(x => x.LookupType)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.DisplayName)
            .ToListAsync(cancellationToken);

    public Task<LookupOption?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.LookupOptions.FirstOrDefaultAsync(x => x.LookupOptionId == id, cancellationToken);

    public Task<bool> ExistsAsync(string lookupType, string code, Guid? excludeId, CancellationToken cancellationToken) =>
        _context.LookupOptions.AnyAsync(x =>
            x.LookupType.ToUpper() == lookupType.ToUpper() &&
            x.Code.ToUpper() == code.ToUpper() &&
            (!excludeId.HasValue || x.LookupOptionId != excludeId.Value), cancellationToken);

    public Task AddAsync(LookupOption option, CancellationToken cancellationToken)
    {
        _context.LookupOptions.Add(option);
        return Task.CompletedTask;
    }

    public void Remove(LookupOption option) => _context.LookupOptions.Remove(option);
}
