using MediatR;

namespace CCAP.Application.Features.Referrals.Commands.UpdateReferralTrackerDetails;

public sealed record UpdateReferralTrackerDetailsCommand(
    Guid ReferralId,
    string? PostedOn,
    string? TrackerStatus,
    DateOnly? EstimatedDischargeDate,
    string? PrimaryInsurance,
    string? Source,
    string? SecondCoordinatorCall,
    string? ThirdCoordinatorCall,
    string? Resolution,
    string? TypeOfReferral) : IRequest;
