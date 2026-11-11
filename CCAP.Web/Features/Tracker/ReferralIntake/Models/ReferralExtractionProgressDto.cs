namespace CCAP.Web.Features.Tracker.ReferralIntake.Models;

public sealed class ReferralExtractionProgressDto
{
    public int TotalPages { get; set; }
    public int PagesProcessed { get; set; }
    public string Stage { get; set; } = "Preparing";
    public bool IsComplete { get; set; }
}
