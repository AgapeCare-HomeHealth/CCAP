namespace CCAP.Application.Features.Referrals.DTOs;

public sealed class CreateReferralTrackerEntryRequest
{
    public DateTime? ReferralDate { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string TypeOfReferral { get; set; } = string.Empty;
    public string? PostedOn { get; set; }
    public DateOnly? EstimatedDischargeDate { get; set; }
    public string? Status { get; set; }
    public string? Source { get; set; }
    public string? Insurance { get; set; }
    public Guid? AssignedCoordinatorId { get; set; }
    public string? AssignedCoordinatorName { get; set; }
    public string? FirstMarketingCall { get; set; }
    public string? SecondCoordinatorCall { get; set; }
    public string? ThirdCoordinatorCall { get; set; }
    public string? Resolution { get; set; }
}
