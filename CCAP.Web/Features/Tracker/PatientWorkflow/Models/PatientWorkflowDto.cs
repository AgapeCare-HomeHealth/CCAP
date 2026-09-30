
namespace CCAP.Web.Features.Tracker.PatientWorkflow.Models;

public class PatientWorkflowDto
{
    public PatientHeaderDto Header { get; set; } = new();

    public List<WorkflowStageDto> WorkflowStages { get; set; } = new();

    public NextActionDto NextAction { get; set; } = new();

    public KeyInformationDto KeyInformation { get; set; } = new();

    public List<ActivityDto> RecentActivities { get; set; } = new();

    public PatientSummaryDto Summary { get; set; } = new();

    public List<ComplianceItemDto> ComplianceItems { get; set; } = new();
    public WorkflowDetailsDto WorkflowDetails { get; set; } = new();

    // =============================================================
    // SOC
    // =============================================================

    public string SocVisitStatus { get; set; } = "Not Scheduled";

    public DateTime? SocScheduledAt { get; set; }

    public DateTime? SocCompletedAt { get; set; }

    public Guid? SocClinicianId { get; set; }

    public string SocClinician { get; set; } = string.Empty;
}
public class WorkflowDetailsDto { public DateOnly? PreAuthDueDate {get;set;} public int? NumberOfVisits {get;set;} public string CaseMixType{get;set;}=""; public string DmeMedSupplyNotes{get;set;}=""; public string SocFeedbackFromPatient{get;set;}=""; public DateOnly? TifDate{get;set;} public DateOnly? RocDate{get;set;} public DateOnly? RecertDate{get;set;} public bool? PcpPtNotified{get;set;} public DateOnly? DischargeDate{get;set;} public string DischargeFeedback{get;set;}=""; public string TransferDestination{get;set;}=""; public DateOnly? TransferDate{get;set;} public string TransferReason{get;set;}=""; }
