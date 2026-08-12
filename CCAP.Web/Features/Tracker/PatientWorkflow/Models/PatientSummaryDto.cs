namespace CCAP.Web.Features.Tracker.PatientWorkflow.Model
{
    public class PatientSummaryDto
    {
        public string PrimaryDiagnosis { get; set; } = string.Empty;

        public string Insurance { get; set; } = string.Empty;

        public DateOnly? SocDate { get; set; }

        public int AuthorizedVisits { get; set; }

        public string Address { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;
    }
}
