using MediatR;

namespace CCAP.Application.Features.Referrals.Commands.UpdateReferralTrackerEntry;

public sealed record UpdateReferralTrackerEntryCommand(
    Guid ReferralId,
    DateTime ReferralDate,
    string FirstName,
    string LastName,
    string? Mrn,
    string TypeOfReferral,
    string? PostedOn,
    DateOnly? EstimatedDischargeDate,
    string? Status,
    string? Source,
    string? Insurance,
    Guid? AssignedCoordinatorId,
    string? FirstMarketingCall,
    string? SecondCoordinatorCall,
    string? ThirdCoordinatorCall,
    string? Resolution) : IRequest;
