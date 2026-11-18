using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Referrals.Commands.UpdateReferralTrackerEntry;

public sealed class UpdateReferralTrackerEntryCommandValidator : IRequestValidator<UpdateReferralTrackerEntryCommand>
{
    public void Validate(UpdateReferralTrackerEntryCommand request)
    {
        if (request.ReferralId == Guid.Empty)
            throw new ArgumentException("Referral ID is required.");
        if (request.ReferralDate == default)
            throw new ArgumentException("Referral date is required.");
        if (string.IsNullOrWhiteSpace(request.FirstName) || request.FirstName.Trim().Length > 100)
            throw new ArgumentException("First name is required and must be 100 characters or fewer.");
        if (string.IsNullOrWhiteSpace(request.LastName) || request.LastName.Trim().Length > 100)
            throw new ArgumentException("Last name is required and must be 100 characters or fewer.");
        if (!CCAP.Domain.Entities.ReferralTrackerEntry.IsValidTypeOfReferral(request.TypeOfReferral))
            throw new ArgumentException("Type of referral must be New Patient or Previous Patient.");

        ValidateLength(request.Mrn, 50, "MRN");
        ValidateLength(request.PostedOn, 50, "Posted on");
        ValidateLength(request.Status, 100, "Status");
        ValidateLength(request.Source, 150, "Referral source");
        ValidateLength(request.Insurance, 150, "Insurance");
        ValidateLength(request.FirstMarketingCall, 2000, "First marketing call");
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
