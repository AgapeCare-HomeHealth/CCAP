using CCAP.Domain.Entities;

namespace CCAP.Application.Abstractions.Persistence;

public interface IReferralDocumentRepository
{
    Task AddAsync(
        ReferralDocument document,
        CancellationToken cancellationToken);
}