namespace CCAP.Web.Features.Tracker.ReferralDrafts.Models;

public sealed class ReferralDraftListItem
{
    public Guid ReferralDraftId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public string SavedByName { get; set; } = string.Empty;

    public string PdfFileName { get; set; } = string.Empty;

    public long? PdfFileSize { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}