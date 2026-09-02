namespace CCAP.Web.Features.Tracker.ReferralDrafts.Models;

public sealed class ReferralDraftListItem
{
    public Guid ReferralDraftId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public string SavedByName { get; set; } = string.Empty;

    public string PatientName { get; set; } = string.Empty;

    public string ReferralNumber { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}