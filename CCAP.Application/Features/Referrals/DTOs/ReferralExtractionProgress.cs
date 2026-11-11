namespace CCAP.Application.Features.Referrals.DTOs;

public sealed class ReferralExtractionProgress
{
    public int TotalPages { get; init; }
    public int PagesProcessed { get; init; }
    public string Stage { get; init; } = "Preparing";
}
