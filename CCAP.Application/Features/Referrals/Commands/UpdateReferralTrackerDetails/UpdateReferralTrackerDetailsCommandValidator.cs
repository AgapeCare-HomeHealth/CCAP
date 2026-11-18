using CCAP.Application.Common.Validation;
using CCAP.Domain.Entities;

namespace CCAP.Application.Features.Referrals.Commands.UpdateReferralTrackerDetails;

public sealed class UpdateReferralTrackerDetailsCommandValidator
    : IRequestValidator<UpdateReferralTrackerDetailsCommand>
{
    public void Validate(UpdateReferralTrackerDetailsCommand request)
    {
        if (request.ReferralId == Guid.Empty)
            throw new ArgumentException("Referral ID is required.");
        if (request.ActorUserId == Guid.Empty)
            throw new ArgumentException("Authenticated user is required.");
        if (!string.IsNullOrWhiteSpace(request.TypeOfReferral) &&
            !ReferralTrackerEntry.IsValidTypeOfReferral(request.TypeOfReferral))
            throw new ArgumentException("Type of referral must be New Patient or Previous Patient.");

        ValidateLength(request.PostedOn, 50, "Posted on");
        ValidateLength(request.TrackerStatus, 100, "Status");
        ValidateLength(request.PrimaryInsurance, 150, "Insurance");
        ValidateLength(request.Source, 150, "Referral source");
        ValidateLength(request.SecondCoordinatorCall, 2000, "Second coordinator call");
        ValidateLength(request.ThirdCoordinatorCall, 2000, "Third coordinator call");
        ValidateLength(request.Resolution, 500, "Resolution");
    }

    private static void ValidateLength(string? value, int maxLength, string fieldName)
    {
        if (value is not null && value.Length > maxLength)
            throw new ArgumentException($"{fieldName} must be {maxLength} characters or fewer.");
    }
}
