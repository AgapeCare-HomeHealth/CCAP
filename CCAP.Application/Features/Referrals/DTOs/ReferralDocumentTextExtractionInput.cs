namespace CCAP.Application.Features.Referrals.DTOs;

/// <summary>
/// Page-local text extracted in the browser from a referral PDF. Page boundaries are
/// retained for MRN source evidence and demographic reconciliation. Values may contain
/// PHI and must not be logged or persisted unnecessarily.
/// </summary>
public sealed class ReferralDocumentTextExtractionInput
{
    public List<string> PageTexts { get; set; } = [];
    public bool OcrUsed { get; set; }
    /// <summary>OCR confidence normalized to the 0-1 range.</summary>
    public double? Confidence { get; set; }
}
