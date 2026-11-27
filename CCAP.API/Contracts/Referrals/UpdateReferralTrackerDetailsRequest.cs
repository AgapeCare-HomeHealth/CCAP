namespace CCAP.API.Contracts.Referrals;

public sealed class UpdateReferralTrackerDetailsRequest
{
    public Guid ReferralId { get; set; }
    public string? PostedOn { get; set; }
    public string? TrackerStatus { get; set; }
    public DateOnly? EstimatedDischargeDate { get; set; }
    public string? PrimaryInsurance { get; set; }
    public string? Source { get; set; }
    public string? SecondCoordinatorCall { get; set; }
    public string? ThirdCoordinatorCall { get; set; }
    public string? Resolution { get; set; }
    public string? TypeOfReferral { get; set; }
}
