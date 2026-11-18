namespace CCAP.API.Contracts.Referrals;

public sealed class ArchiveReferralTrackerEntriesRequest
{
    public List<Guid> ReferralIds { get; set; } = [];
}
