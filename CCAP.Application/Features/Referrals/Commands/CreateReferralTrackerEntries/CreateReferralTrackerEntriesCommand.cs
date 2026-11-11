using CCAP.Application.Features.Referrals.DTOs;
using MediatR;

namespace CCAP.Application.Features.Referrals.Commands.CreateReferralTrackerEntries;

public sealed record CreateReferralTrackerEntriesCommand(
    IReadOnlyCollection<CreateReferralTrackerEntryRequest> Entries)
    : IRequest<CreateReferralTrackerEntriesResult>;
