using MediatR;

namespace CCAP.Application.Features.Referrals.Commands.ArchiveReferralTrackerEntry;

public sealed record ArchiveReferralTrackerEntryCommand(Guid ReferralId, Guid ArchivedByUserId) : IRequest;
