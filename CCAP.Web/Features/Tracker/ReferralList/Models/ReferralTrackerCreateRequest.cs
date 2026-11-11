namespace CCAP.Web.Features.Tracker.ReferralList.Models;

public sealed class ReferralTrackerCreateRequest
{
    public DateTime? ReferralDate { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string? Mrn { get; set; }
    public string? PostedOn { get; set; }
    public DateOnly? EstimatedDischargeDate { get; set; }
    public string? Status { get; set; }
    public string? Source { get; set; }
    public string? Insurance { get; set; }
    public Guid? AssignedCoordinatorId { get; set; }
    public string? FirstMarketingCall { get; set; }
    public string? SecondCoordinatorCall { get; set; }
    public string? ThirdCoordinatorCall { get; set; }
    public string? Resolution { get; set; }
}
