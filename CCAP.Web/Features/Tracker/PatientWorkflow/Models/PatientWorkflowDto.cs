using CCAP.Web.Features.Tracker.PatientWorkflow.Models;

namespace CCAP.Web.Features.Tracker.PatientWorkflow.Model
{
    public class PatientWorkflowDto
    {
        public PatientHeaderDto Header { get; set; } = new();

        public List<WorkflowStageDto> WorkflowStages { get; set; } = new();

        public NextActionDto NextAction { get; set; } = new();

        public KeyInformationDto KeyInformation { get; set; } = new();

        public List<ActivityDto> RecentActivities { get; set; } = new();

        public PatientSummaryDto Summary { get; set; } = new();
    }
}
