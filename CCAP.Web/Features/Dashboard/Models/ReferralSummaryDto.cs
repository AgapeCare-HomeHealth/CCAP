namespace CCAP.Web.Features.Dashboard.Models
{
    public class ReferralSummaryDto
    {
        public string PatientName { get; set; } = string.Empty;
        public string ReferralNumber { get; set; } = string.Empty;

        public DateTime ReferralDate { get; set; }
    }
}
