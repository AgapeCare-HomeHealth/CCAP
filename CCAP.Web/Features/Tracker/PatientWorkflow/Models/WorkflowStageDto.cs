using CCAP.Web.Features.Tracker.PatientWorkflow.Model;

namespace CCAP.Web.Features.Tracker.PatientWorkflow.Models;

public class WorkflowStageDto
{
    public int Sequence { get; set; }

    public string StageCode { get; set; } = string.Empty;

    public string StageName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public WorkflowStatus Status { get; set; }

    public DateTime? CompletedDate { get; set; }

    public Guid? AssignedUserId { get; set; }

    public string AssignedUserName { get; set; } = string.Empty;

    public bool IsClickable { get; set; }

    public string Route { get; set; } = string.Empty;
}

