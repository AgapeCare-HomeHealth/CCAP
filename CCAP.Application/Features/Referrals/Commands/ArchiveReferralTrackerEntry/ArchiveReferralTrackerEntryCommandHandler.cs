using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Referrals.Commands.ArchiveReferralTrackerEntry;

public sealed class ArchiveReferralTrackerEntryCommandHandler : IRequestHandler<ArchiveReferralTrackerEntryCommand>
{
    private readonly IReferralTrackerRepository _referrals;
    private readonly IUnitOfWork _unitOfWork;

    public ArchiveReferralTrackerEntryCommandHandler(
        IReferralTrackerRepository referrals,
        IUnitOfWork unitOfWork)
    {
        _referrals = referrals;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ArchiveReferralTrackerEntryCommand request, CancellationToken cancellationToken)
    {
        var referral = await _referrals.GetByIdAsync(request.ReferralId, cancellationToken)
            ?? throw new KeyNotFoundException("Referral tracker entry was not found.");

        referral.Archive(request.ArchivedByUserId);
        _referrals.Update(referral);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
