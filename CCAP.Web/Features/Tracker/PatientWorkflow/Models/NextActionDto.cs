namespace CCAP.Web.Features.Tracker.PatientWorkflow.Model
{
    public class NextActionDto
    {
        public Guid TaskId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime DueDate { get; set; }

        public string PageRoute { get; set; } = string.Empty;

        public string Icon { get; set; } = string.Empty;

        public bool IsOverdue { get; set; }
    }
}
