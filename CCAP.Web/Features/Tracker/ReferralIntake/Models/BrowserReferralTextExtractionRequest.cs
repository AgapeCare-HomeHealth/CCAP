namespace CCAP.Web.Features.Tracker.ReferralIntake.Models;

/// <summary>Web-to-API request containing only page text produced by browser-side OCR.</summary>
public sealed class BrowserReferralTextExtractionRequest
{
    public List<string> PageTexts { get; set; } = [];
    public bool OcrUsed { get; set; }
    /// <summary>OCR confidence normalized to the 0-1 range.</summary>
    public double? Confidence { get; set; }
}
