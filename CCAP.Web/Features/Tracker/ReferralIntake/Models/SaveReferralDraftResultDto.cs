namespace CCAP.Web.Features.Tracker.ReferralIntake.Models;

public sealed class SaveReferralDraftResultDto
{
    public Guid ReferralDraftId { get; set; }

    public string Status { get; set; }
        = string.Empty;
}