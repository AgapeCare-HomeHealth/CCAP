using CCAP.Domain.Entities;
using CCAP.Application.Features.Referrals.DTOs;

namespace CCAP.Application.Abstractions.Persistence;

public interface IReferralTrackerRepository
{
    Task<(IReadOnlyList<ReferralListItemDto> Items, int TotalCount)> GetPagedListAsync(
        int pageNumber, int pageSize, string? search, string? status, string? source, string? insurance,
        CancellationToken cancellationToken);
    Task<ReferralTrackerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(ReferralTrackerEntry entry, CancellationToken cancellationToken);
    void Update(ReferralTrackerEntry entry);
}
