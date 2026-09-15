namespace CCAP.Web.Features.Tracker.ReferralIntake.Models;

/// <summary>
/// Data that is intentionally persisted by Save Draft.
/// Drafts represent Step 1 only; later wizard steps are never stored here.
/// </summary>
public sealed class ReferralIntakeDraftData
{
    public string? ReferralPdfFileName { get; set; }
    public string? ReferralPdfContentType { get; set; }
    public long? ReferralPdfSize { get; set; }
}
