using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Referrals.DTOs;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class ReferralTrackerRepository(AppDbContext context) : IReferralTrackerRepository
{
    public async Task<(IReadOnlyList<ReferralListItemDto> Items, int TotalCount)> GetPagedListAsync(
        int pageNumber, int pageSize, string? search, string? status, string? source, string? insurance,
        CancellationToken cancellationToken)
    {
        var query = context.ReferralTrackerEntries.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.ReferralNumber.Contains(term) || x.Mrn.Contains(term)
                || x.FirstName.Contains(term) || x.LastName.Contains(term));
        }
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(x => x.Status == status.Trim());
        if (!string.IsNullOrWhiteSpace(source)) query = query.Where(x => x.Source.Contains(source.Trim()));
        if (!string.IsNullOrWhiteSpace(insurance)) query = query.Where(x => x.Insurance.Contains(insurance.Trim()));

        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.ReferralDate)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new ReferralListItemDto
            {
                ReferralId = x.ReferralTrackerEntryId,
                ReferralNumber = x.ReferralNumber,
                ReferralDate = x.ReferralDate,
                PatientName = (x.FirstName + " " + x.LastName).Trim(),
                Mrn = x.Mrn,
                TypeOfReferral = x.TypeOfReferral,
                Status = x.Status,
                Source = x.Source,
                Insurance = x.Insurance,
                CareCoordinator = x.AssignedCoordinatorName ?? string.Empty,
                TrackerCoordinatorName = x.AssignedCoordinatorName ?? string.Empty,
                AssignedCoordinatorId = x.AssignedCoordinatorId,
                PostedOn = x.PostedOn,
                EstimatedDischargeDate = x.EstimatedDischargeDate,
                FirstMarketingCall = x.FirstMarketingCall,
                SecondCoordinatorCall = x.SecondCoordinatorCall,
                ThirdCoordinatorCall = x.ThirdCoordinatorCall,
                Resolution = x.Resolution
            }).ToListAsync(cancellationToken);
        return (items, count);
    }

    public Task<ReferralTrackerEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.ReferralTrackerEntries.FirstOrDefaultAsync(x => x.ReferralTrackerEntryId == id, cancellationToken);

    public Task AddAsync(ReferralTrackerEntry entry, CancellationToken cancellationToken) =>
        context.ReferralTrackerEntries.AddAsync(entry, cancellationToken).AsTask();

    public void Update(ReferralTrackerEntry entry) => context.ReferralTrackerEntries.Update(entry);
}
