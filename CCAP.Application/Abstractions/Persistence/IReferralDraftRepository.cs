using CCAP.Application.Features.ReferralDrafts.ReadModels;
using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IReferralDraftRepository
{
    Task AddAsync(
        ReferralDraft draft,
        CancellationToken cancellationToken);

    Task<ReferralDraft?> GetByIdAsync(
        Guid draftId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ReferralDraft>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ReferralDraft>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<(IReadOnlyList<ReferralDraftListReadModel> Items, int TotalCount)>
        GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? search,
            CancellationToken cancellationToken);
}