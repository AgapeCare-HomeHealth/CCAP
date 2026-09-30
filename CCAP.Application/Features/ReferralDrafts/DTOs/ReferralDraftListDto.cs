namespace CCAP.Application.Features.ReferralDrafts.DTOs;

public sealed class ReferralDraftListDto
{
    public Guid ReferralDraftId { get; init; }

    public Guid CreatedByUserId { get; init; }

    public string SavedByName { get; init; } = string.Empty;

    public string PdfFileName { get; init; } = string.Empty;

    public long? PdfFileSize { get; init; }

    public string Status { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}