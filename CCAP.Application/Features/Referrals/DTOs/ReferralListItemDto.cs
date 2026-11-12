namespace CCAP.Application.Features.Referrals.DTOs;

public sealed class ReferralListItemDto
{
    public Guid ReferralId { get; set; }
    public string ReferralNumber { get; set; } = string.Empty;
    public DateTime ReferralDate { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string Mrn { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Insurance { get; set; } = string.Empty;
    public string CareCoordinator { get; set; } = string.Empty;
    public string TrackerCoordinatorName { get; set; } = string.Empty;
    public Guid? AssignedCoordinatorId { get; set; }
    public string PostedOn { get; set; } = string.Empty;
    public DateOnly? EstimatedDischargeDate { get; set; }
    public string FirstMarketingCall { get; set; } = string.Empty;
    public string SecondCoordinatorCall { get; set; } = string.Empty;
    public string ThirdCoordinatorCall { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;
}
