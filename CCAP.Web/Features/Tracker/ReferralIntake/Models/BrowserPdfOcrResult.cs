namespace CCAP.Web.Features.Tracker.ReferralIntake.Models;

/// <summary>Result returned by browser-side PDF.js/Tesseract.js processing.</summary>
public sealed class BrowserPdfOcrResult
{
    public List<string> PageTexts { get; set; } = [];
    public bool OcrUsed { get; set; }
    public double? Confidence { get; set; }
    public int PageCount { get; set; }
}
