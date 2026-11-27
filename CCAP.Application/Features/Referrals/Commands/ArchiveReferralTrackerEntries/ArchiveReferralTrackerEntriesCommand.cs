using MediatR;

namespace CCAP.Application.Features.Referrals.Commands.ArchiveReferralTrackerEntries;

public sealed record ArchiveReferralTrackerEntriesCommand(
    IReadOnlyCollection<Guid> ReferralIds,
    Guid ArchivedByUserId) : IRequest;
