using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;

namespace CCAP.Infrastructure.Persistence.Repositories;

public sealed class ReferralDocumentRepository
    : IReferralDocumentRepository
{
    private readonly AppDbContext _context;

    public ReferralDocumentRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(
        ReferralDocument document,
        CancellationToken cancellationToken) =>
        _context.ReferralDocuments
            .AddAsync(
                document,
                cancellationToken)
            .AsTask();
}