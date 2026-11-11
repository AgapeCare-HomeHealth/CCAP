namespace CCAP.API.Contracts.Referrals;

public sealed class ReferralExtractionProgressResponse
{
    public int TotalPages { get; init; }
    public int PagesProcessed { get; init; }
    public string Stage { get; init; } = "Preparing";
    public bool IsComplete { get; init; }
}
