using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Referrals.Commands.ArchiveReferralTrackerEntry;

public sealed class ArchiveReferralTrackerEntryCommandValidator : IRequestValidator<ArchiveReferralTrackerEntryCommand>
{
    public void Validate(ArchiveReferralTrackerEntryCommand request)
    {
        if (request.ReferralId == Guid.Empty)
            throw new ArgumentException("Referral ID is required.");
        if (request.ArchivedByUserId == Guid.Empty)
            throw new ArgumentException("Archiving user is required.");
    }
}
