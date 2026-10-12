namespace CCAP.Web.Features.Dashboard.Models
{
    public class UpcomingVisitDto
    {
        public string PatientName { get; set; } = string.Empty;
        public string Clinician { get; set; } = string.Empty;

        public DateTime VisitDate { get; set; }
    }
}
