using CCAP.Domain.Entities;
using CCAP.Application.Features.Referrals.DTOs;

namespace CCAP.Application.Abstractions.Persistence;

public interface IReferralRepository
{
    Task<Referral?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<bool> ExistsByReferralNumberAsync(
        string referralNumber,
        CancellationToken cancellationToken);

    Task AddAsync(
        Referral referral,
        CancellationToken cancellationToken);

    void Update(Referral referral);

    Task<(IReadOnlyList<ReferralListItemDto> Items, int TotalCount)> GetPagedListAsync(
        int pageNumber, int pageSize, string? search, string? status, string? source,
        string? insurance, Guid? coordinatorId, DateTime? fromDate, DateTime? toDate,
        CancellationToken cancellationToken);
}