using CCAP.Application.Features.Referrals.DTOs;

namespace CCAP.Application.Abstractions.Referrals;

public interface IReferralDocumentExtractionService
{
    Task<ReferralDocumentExtractionResult> ExtractAsync(
        Stream pdfStream,
        CancellationToken cancellationToken = default);

    Task<ReferralDocumentExtractionResult> ExtractFromTextAsync(
        IReadOnlyList<string> pageTexts,
        bool ocrUsed,
        double? confidence,
        CancellationToken cancellationToken = default);
}
