using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Referrals.Commands.UpdateReferralTrackerDetails;

public sealed class UpdateReferralTrackerDetailsCommandHandler
    : IRequestHandler<UpdateReferralTrackerDetailsCommand>
{
    private readonly IReferralTrackerRepository _referrals;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReferralTrackerDetailsCommandHandler(
        IReferralTrackerRepository referrals,
        IUnitOfWork unitOfWork)
    {
        _referrals = referrals;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateReferralTrackerDetailsCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ActorUserId == Guid.Empty)
            throw new UnauthorizedAccessException("Unable to determine the authenticated user.");

        var referral = await _referrals.GetByIdAsync(request.ReferralId, cancellationToken)
            ?? throw new KeyNotFoundException("Referral tracker entry was not found.");

        if (!request.IsAdministrator && referral.AssignedCoordinatorId != request.ActorUserId)
            throw new UnauthorizedAccessException("Only the assigned Care Coordinator or an Administrator can edit this referral.");

        referral.Update(
            request.PostedOn,
            request.EstimatedDischargeDate,
            request.TrackerStatus,
            request.Source,
            request.PrimaryInsurance,
            referral.AssignedCoordinatorId,
            referral.AssignedCoordinatorName,
            referral.FirstMarketingCall,
            request.SecondCoordinatorCall,
            request.ThirdCoordinatorCall,
            request.Resolution,
            request.TypeOfReferral);

        _referrals.Update(referral);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
