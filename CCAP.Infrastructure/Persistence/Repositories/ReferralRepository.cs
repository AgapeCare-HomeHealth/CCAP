using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using CCAP.Application.Features.Referrals.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class ReferralRepository : IReferralRepository
{
    private readonly AppDbContext _context;
    public ReferralRepository(AppDbContext context) => _context = context;

    public Task<Referral?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.Referrals
            .Include(x => x.Patient)
            .Include(x => x.AssignedUser)
            .FirstOrDefaultAsync(x => x.ReferralId == id, cancellationToken);

    public Task<bool> ExistsByReferralNumberAsync(
    string referralNumber,
    CancellationToken cancellationToken) =>
    _context.Referrals.AnyAsync(
        x => x.ReferralNumber == referralNumber.Trim(),
        cancellationToken);

    public Task AddAsync(Referral referral, CancellationToken cancellationToken) =>
        _context.Referrals.AddAsync(referral, cancellationToken).AsTask();

    public void Update(Referral referral) => _context.Referrals.Update(referral);

    public async Task<(IReadOnlyList<ReferralListItemDto> Items, int TotalCount)> GetPagedListAsync(
        int pageNumber, int pageSize, string? search, string? status, string? source,
        string? insurance, Guid? coordinatorId, DateTime? fromDate, DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var query = _context.Referrals.AsNoTracking()
            .Include(x => x.Patient)
                .ThenInclude(x => x!.Coordinator)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.ReferralNumber.Contains(term) ||
                (x.Patient != null && (x.Patient.MRN.Contains(term) ||
                 x.Patient.FirstName.Contains(term) || x.Patient.LastName.Contains(term))));
        }
        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim();
            if (Enum.TryParse<CCAP.Domain.Enums.ReferralStatus>(normalizedStatus, true, out var parsedStatus))
            {
                query = query.Where(x => x.Status == parsedStatus ||
                    x.TrackerStatus == normalizedStatus ||
                    (x.Patient != null && x.Patient.FinalStatus == normalizedStatus));
            }
            else
            {
                query = query.Where(x => x.TrackerStatus == normalizedStatus ||
                    (x.Patient != null && x.Patient.FinalStatus == normalizedStatus));
            }
        }
        if (!string.IsNullOrWhiteSpace(source)) query = query.Where(x => x.Source != null && x.Source.Contains(source.Trim()));
        if (!string.IsNullOrWhiteSpace(insurance)) query = query.Where(x =>
            (x.PrimaryInsurance != null && x.PrimaryInsurance.Contains(insurance.Trim())) ||
            (x.Patient != null && x.Patient.PrimaryInsurance != null && x.Patient.PrimaryInsurance.Contains(insurance.Trim())));
        if (coordinatorId.HasValue) query = query.Where(x => x.Patient != null && x.Patient.CoordinatorId == coordinatorId.Value);
        if (fromDate.HasValue) query = query.Where(x => x.ReferralDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(x => x.ReferralDate < toDate.Value.Date.AddDays(1));

        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.ReferralDate)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new ReferralListItemDto
            {
                ReferralId = x.ReferralId,
                ReferralNumber = x.ReferralNumber,
                ReferralDate = x.ReferralDate,
                PatientName = x.Patient == null ? "Unlinked patient" :
                    (x.Patient.FirstName + " " + x.Patient.LastName).Trim(),
                Mrn = x.Patient == null ? string.Empty : x.Patient.MRN,
                Status = x.TrackerStatus ??
                    (x.Patient != null && !string.IsNullOrWhiteSpace(x.Patient.FinalStatus)
                        ? x.Patient.FinalStatus
                        : x.Status.ToString()),
                Source = x.Source ?? string.Empty,
                Insurance = x.PrimaryInsurance ?? (x.Patient == null ? string.Empty : x.Patient.PrimaryInsurance ?? string.Empty),
                CareCoordinator = x.TrackerCoordinatorName ??
                    (x.Patient == null || x.Patient.Coordinator == null ? string.Empty :
                        (x.Patient.Coordinator.FirstName + " " + x.Patient.Coordinator.LastName).Trim()),
                TrackerCoordinatorName = x.TrackerCoordinatorName ?? string.Empty,
                // Current intake submissions enter this tracker as Referral/Intake.
                PostedOn = x.PostedOn ?? "Referral/Intake",
                EstimatedDischargeDate = x.EstimatedDischargeDate,
                FirstMarketingCall = x.FirstMarketingCall ?? (x.Patient == null
                    ? string.Empty
                    : (x.Patient.CallNotes.Where(n => n.ContactType.ToLower() == "marketing")
                        .OrderBy(n => n.CallDate).Select(n => n.Notes).FirstOrDefault() ?? string.Empty)),
                SecondCoordinatorCall = x.SecondCoordinatorCall ?? (x.Patient == null
                    ? string.Empty
                    : (x.Patient.CallNotes.Where(n => n.ContactType.ToLower() == "cc")
                        .OrderBy(n => n.CallDate).Select(n => n.Notes).FirstOrDefault() ?? string.Empty)),
                ThirdCoordinatorCall = x.ThirdCoordinatorCall ?? (x.Patient == null
                    ? string.Empty
                    : (x.Patient.CallNotes.Where(n => n.ContactType.ToLower() == "cc")
                        .OrderBy(n => n.CallDate).Select(n => n.Notes).Skip(1).FirstOrDefault() ?? string.Empty)),
                Resolution = x.Resolution ?? string.Empty
            }).ToListAsync(cancellationToken);
        return (items, count);
    }

}
