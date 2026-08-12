using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IReferralRepository
{
    Task<Referral?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(Referral referral, CancellationToken cancellationToken);
    void Update(Referral referral);
}
