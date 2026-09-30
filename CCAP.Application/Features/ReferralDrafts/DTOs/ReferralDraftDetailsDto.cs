namespace CCAP.Application.Features.ReferralDrafts.DTOs;

public sealed class ReferralDraftDetailsDto
{
    public Guid ReferralDraftId { get; init; }

    public string Data { get; init; } = string.Empty;
}