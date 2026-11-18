using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Referrals.Commands.ArchiveReferralTrackerEntries;

public sealed class ArchiveReferralTrackerEntriesCommandValidator
    : IRequestValidator<ArchiveReferralTrackerEntriesCommand>
{
    public void Validate(ArchiveReferralTrackerEntriesCommand request)
    {
        if (request.ArchivedByUserId == Guid.Empty)
            throw new ArgumentException("Archiving user is required.");
        if (request.ReferralIds is null || request.ReferralIds.Count == 0)
            throw new ArgumentException("Select at least one referral to archive.");
        if (request.ReferralIds.Count > 100)
            throw new ArgumentException("No more than 100 referrals can be archived at once.");
        if (request.ReferralIds.Any(id => id == Guid.Empty))
            throw new ArgumentException("Every referral ID must be valid.");
        if (request.ReferralIds.Distinct().Count() != request.ReferralIds.Count)
            throw new ArgumentException("Duplicate referrals cannot be archived.");
    }
}
