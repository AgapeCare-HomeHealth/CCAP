using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Referrals.Commands.ArchiveReferralTrackerEntries;

public sealed class ArchiveReferralTrackerEntriesCommandHandler : IRequestHandler<ArchiveReferralTrackerEntriesCommand>
{
    private readonly IReferralTrackerRepository _referrals;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveReferralTrackerEntriesCommandHandler(
        IReferralTrackerRepository referrals,
        IUnitOfWork unitOfWork)
    {
        _referrals = referrals;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ArchiveReferralTrackerEntriesCommand request, CancellationToken cancellationToken)
    {
        var entries = new List<CCAP.Domain.Entities.ReferralTrackerEntry>(request.ReferralIds.Count);
        foreach (var referralId in request.ReferralIds)
        {
            var referral = await _referrals.GetByIdAsync(referralId, cancellationToken)
                ?? throw new KeyNotFoundException("One or more selected referral tracker entries were not found or are already archived.");
            entries.Add(referral);
        }

        foreach (var referral in entries)
        {
            referral.Archive(request.ArchivedByUserId);
            _referrals.Update(referral);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
