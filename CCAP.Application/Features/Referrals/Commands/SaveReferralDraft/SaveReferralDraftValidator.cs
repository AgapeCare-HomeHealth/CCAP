namespace CCAP.Application.Features.Referrals.Commands.SaveReferralDraft;

public static class SaveReferralDraftValidator
{
    public static void Validate(
        SaveReferralDraftCommand request)
    {
        if (request.CreatedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Created by user is required.");
        }

        if (string.IsNullOrWhiteSpace(
                request.Data))
        {
            throw new ArgumentException(
                "Draft data cannot be empty.");
        }
    }
}