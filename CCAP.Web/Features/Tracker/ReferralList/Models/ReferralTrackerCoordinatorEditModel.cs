namespace CCAP.Web.Features.Tracker.ReferralList.Models;

public sealed class ReferralTrackerCoordinatorEditModel
{
    public Guid ReferralId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PostedOn { get; set; } = string.Empty;
    public DateOnly? EstimatedDischargeDate { get; set; }
    public string? TypeOfReferral { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Insurance { get; set; } = string.Empty;
    public string SecondCoordinatorCall { get; set; } = string.Empty;
    public string ThirdCoordinatorCall { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;

    public static ReferralTrackerCoordinatorEditModel From(ReferralListItem item) => new()
    {
        ReferralId = item.ReferralId,
        PatientName = item.PatientName,
        PostedOn = item.PostedOn,
        EstimatedDischargeDate = item.EstimatedDischargeDate,
        TypeOfReferral = item.TypeOfReferral,
        Status = item.Status,
        Source = item.Source,
        Insurance = item.Insurance,
        SecondCoordinatorCall = item.SecondCoordinatorCall,
        ThirdCoordinatorCall = item.ThirdCoordinatorCall,
        Resolution = item.Resolution
    };
}
