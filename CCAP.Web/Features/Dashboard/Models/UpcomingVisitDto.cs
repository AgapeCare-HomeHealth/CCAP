namespace CCAP.Web.Features.Dashboard.Models
{
    public class UpcomingVisitDto
    {
        public string PatientName { get; set; }
        public string Clinician { get; set; }

        public DateTime VisitDate { get; set; }
    }
}
