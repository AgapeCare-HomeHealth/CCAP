namespace CCAP.Web.Features.Tracker.ReferralList.Models;

public sealed class ReferralTrackerAdminEditModel
{
    public Guid ReferralId { get; set; }
    public string ReferralNumber { get; set; } = string.Empty;
    public DateTime ReferralDate { get; set; } = DateTime.Today;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Mrn { get; set; } = string.Empty;
    public string TypeOfReferral { get; set; } = "New Patient";
    public string PostedOn { get; set; } = "Referral/Intake";
    public DateOnly? EstimatedDischargeDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Insurance { get; set; } = string.Empty;
    public Guid? AssignedCoordinatorId { get; set; }
    public string AssignedCoordinatorName { get; set; } = string.Empty;
    public string FirstMarketingCall { get; set; } = string.Empty;
    public string SecondCoordinatorCall { get; set; } = string.Empty;
    public string ThirdCoordinatorCall { get; set; } = string.Empty;
    public string Resolution { get; set; } = string.Empty;

    public static ReferralTrackerAdminEditModel From(ReferralListItem item) => new()
    {
        ReferralId = item.ReferralId,
        ReferralNumber = item.ReferralNumber,
        ReferralDate = item.ReferralDate,
        FirstName = item.FirstName,
        LastName = item.LastName,
        Mrn = item.Mrn,
        TypeOfReferral = item.TypeOfReferral ?? string.Empty,
        PostedOn = item.PostedOn,
        EstimatedDischargeDate = item.EstimatedDischargeDate,
        Status = item.Status,
        Source = item.Source,
        Insurance = item.Insurance,
        AssignedCoordinatorId = item.AssignedCoordinatorId,
        AssignedCoordinatorName = item.CareCoordinator,
        FirstMarketingCall = item.FirstMarketingCall,
        SecondCoordinatorCall = item.SecondCoordinatorCall,
        ThirdCoordinatorCall = item.ThirdCoordinatorCall,
        Resolution = item.Resolution
    };
}
