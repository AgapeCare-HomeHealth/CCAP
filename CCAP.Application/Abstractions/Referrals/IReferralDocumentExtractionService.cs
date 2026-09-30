using CCAP.Application.Features.Referrals.DTOs;

namespace CCAP.Application.Abstractions.Referrals;

public interface IReferralDocumentExtractionService
{
    Task<ReferralDocumentExtractionResult> ExtractAsync(
        Stream pdfStream,
        CancellationToken cancellationToken = default);
}
