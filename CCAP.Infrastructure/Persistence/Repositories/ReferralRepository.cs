using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
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
}
