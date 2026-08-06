namespace CCAP.Web.Features.Dashboard.Models
{
    public class MyTaskDto
    {
        public Guid TransactionId { get; set; }

        public string Priority { get; set; } = "";

        public string TaskName { get; set; } = "";

        public string PatientName { get; set; } = "";

        public DateTime DueDate { get; set; }

        public string AssignedTo { get; set; } = "";

        public string CurrentStage { get; set; } = "";
    }
}
