using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface ILookupOptionRepository
{
    Task<string?> GetCanonicalLookupTypeAsync(string lookupType, CancellationToken cancellationToken);
    Task<List<LookupOption>> GetAsync(string? lookupType, bool activeOnly, CancellationToken cancellationToken);
    Task<LookupOption?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string lookupType, string code, Guid? excludeId, CancellationToken cancellationToken);
    Task AddAsync(LookupOption option, CancellationToken cancellationToken);
    void Remove(LookupOption option);
}
