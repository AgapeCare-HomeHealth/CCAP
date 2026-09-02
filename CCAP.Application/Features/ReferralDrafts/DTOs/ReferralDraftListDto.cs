namespace CCAP.Application.Features.ReferralDrafts.DTOs;

public sealed class ReferralDraftListDto
{
    public Guid ReferralDraftId { get; init; }

    public Guid CreatedByUserId { get; init; }

    public string SavedByName { get; init; } = string.Empty;

    public string PatientName { get; init; } = string.Empty;

    public string ReferralNumber { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }
}