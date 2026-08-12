namespace CCAP.Web.Features.Tracker.PatientWorkflow.Model
{
    public class ActivityDto
    {
        public Guid ActivityId { get; set; }

        public DateTime ActivityDate { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string PerformedBy { get; set; } = string.Empty;

        public string ActivityType { get; set; } = string.Empty;
    }
}
