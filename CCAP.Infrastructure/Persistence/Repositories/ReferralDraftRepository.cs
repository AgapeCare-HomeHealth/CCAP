using CCAP.Application.Features.ReferralDrafts.ReadModels;
using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class ReferralDraftRepository
    : IReferralDraftRepository
{
    private readonly AppDbContext _context;

    public ReferralDraftRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(
        ReferralDraft draft,
        CancellationToken cancellationToken)
    {
        await _context.ReferralDrafts.AddAsync(
            draft,
            cancellationToken);
    }

    public async Task<ReferralDraft?> GetByIdAsync(
        Guid draftId,
        CancellationToken cancellationToken)
    {
        return await _context.ReferralDrafts
            .FirstOrDefaultAsync(
                x => x.ReferralDraftId == draftId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<ReferralDraft>>
        GetForUserAsync(
            Guid userId,
            CancellationToken cancellationToken)
    {
        return await _context.ReferralDrafts
            .Where(x =>
                x.CreatedByUserId == userId)
            .OrderByDescending(
                x => x.UpdatedAt)
            .ToListAsync(
                cancellationToken);
    }

    public async Task<IReadOnlyList<ReferralDraft>>
        GetAllAsync(
            CancellationToken cancellationToken)
    {
        return await _context.ReferralDrafts
            .OrderByDescending(
                x => x.UpdatedAt)
            .ToListAsync(
                cancellationToken);
    }

    //public async Task<(
    //IReadOnlyList<ReferralDraft> Items,
    //int TotalCount)>
    //GetPagedAsync(
    //    int pageNumber,
    //    int pageSize,
    //    string? search,
    //    CancellationToken cancellationToken)
    //{
    //    var query =
    //        _context.ReferralDrafts
    //            .AsNoTracking();

    //    if (!string.IsNullOrWhiteSpace(search))
    //    {
    //        var searchTerm =
    //            search.Trim();

    //        query =
    //            query.Where(x =>
    //                x.Data.Contains(searchTerm));
    //    }

    //    var totalCount =
    //        await query.CountAsync(
    //            cancellationToken);

    //    var items =
    //        await query
    //            .OrderByDescending(
    //                x => x.UpdatedAt)
    //            .ThenByDescending(
    //                x => x.ReferralDraftId)
    //            .Skip(
    //                (pageNumber - 1) *
    //                pageSize)
    //            .Take(
    //                pageSize)
    //            .ToListAsync(
    //                cancellationToken);

    //    return (
    //        items,
    //        totalCount);
    //}

    public async Task<(
    IReadOnlyList<ReferralDraftListReadModel> Items,
    int TotalCount)>
    GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? search,
        CancellationToken cancellationToken)
    {
        //var query =
        //    from draft in _context.ReferralDrafts.AsNoTracking()
        //    join user in _context.ApplicationUsers.AsNoTracking()
        //        on draft.CreatedByUserId equals user.UserId
        //    select new
        //    {
        //        Draft = draft,

        //        CreatedByName =
        //            (user.FirstName + " " + user.LastName).Trim()
        //    };

        var query =
            from draft in _context.ReferralDrafts.AsNoTracking()

            join user in _context.ApplicationUsers.AsNoTracking()
                on draft.CreatedByUserId equals user.UserId
                into users

            from user in users.DefaultIfEmpty()

            select new
            {
                Draft = draft,

                CreatedByName =
                    user == null
                        ? "Unknown User"
                        : (user.FirstName + " " + user.LastName).Trim()
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm =
                search.Trim();

            query =
                query.Where(x =>
                    x.Draft.Data.Contains(searchTerm) ||
                    x.CreatedByName.Contains(searchTerm));
        }

        var totalCount =
            await query.CountAsync(
                cancellationToken);

        var rows =
            await query
                .OrderByDescending(
                    x => x.Draft.UpdatedAt)
                .ThenByDescending(
                    x => x.Draft.ReferralDraftId)
                .Skip(
                    (pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(
                    cancellationToken);

        var items =
            rows
                .Select(x =>
                    new ReferralDraftListReadModel(
                        x.Draft,
                        x.CreatedByName))
                .ToList();

        return (
            items,
            totalCount);
    }
}