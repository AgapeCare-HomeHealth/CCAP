using CCAP.Application.Abstractions.Persistence;
using MediatR;

namespace CCAP.Application.Features.Referrals.Commands.UpdateReferralTrackerEntry;

public sealed class UpdateReferralTrackerEntryCommandHandler : IRequestHandler<UpdateReferralTrackerEntryCommand>
{
    private readonly IReferralTrackerRepository _referrals;
    private readonly IUserRepository _users;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateReferralTrackerEntryCommandHandler(
        IReferralTrackerRepository referrals,
        IUserRepository users,
        IUnitOfWork unitOfWork)
    {
        _referrals = referrals;
        _users = users;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateReferralTrackerEntryCommand request, CancellationToken cancellationToken)
    {
        var referral = await _referrals.GetByIdAsync(request.ReferralId, cancellationToken)
            ?? throw new KeyNotFoundException("Referral tracker entry was not found.");

        string? coordinatorName = null;
        if (request.AssignedCoordinatorId is Guid coordinatorId)
        {
            if (coordinatorId == referral.AssignedCoordinatorId)
            {
                coordinatorName = referral.AssignedCoordinatorName;
            }
            else
            {
                var coordinators = await _users.GetByIdsAsync([coordinatorId], cancellationToken);
                var coordinator = coordinators.SingleOrDefault();
                if (coordinator is null || !coordinator.IsActive ||
                    string.IsNullOrWhiteSpace(coordinator.Email) ||
                    string.IsNullOrWhiteSpace(coordinator.PasswordHash) ||
                    coordinator.Role is null || !coordinator.Role.IsActive ||
                    !coordinator.Role.RoleName.Contains("Coordinator", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ArgumentException("The assigned user must be an active registered Care Coordinator.");
                }

                coordinatorName = $"{coordinator.FirstName} {coordinator.LastName}".Trim();
            }
        }

        referral.UpdateAllTrackerDetails(
            request.ReferralDate,
            request.FirstName,
            request.LastName,
            request.Mrn,
            request.TypeOfReferral,
            request.PostedOn,
            request.EstimatedDischargeDate,
            request.Status,
            request.Source,
            request.Insurance,
            request.AssignedCoordinatorId,
            coordinatorName,
            request.FirstMarketingCall,
            request.SecondCoordinatorCall,
            request.ThirdCoordinatorCall,
            request.Resolution);

        _referrals.Update(referral);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
