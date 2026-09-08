namespace CCAP.Web.Features.Tracker.PatientWorkflow.Model
{
    public class PatientSummaryDto
    {
        public string PrimaryDiagnosis { get; set; } = string.Empty;

        public string Insurance { get; set; } = string.Empty;

        public string InsuranceMemberId { get; set; } = string.Empty;

        public DateOnly? AuthorizationDate { get; set; }

        public int? AuthorizedVisits { get; set; }

        public bool AuthorizationRequired { get; set; }

        public DateOnly? SocDate { get; set; }

        public string Address { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        // =========================================================
        // INSURANCE VERIFICATION
        // =========================================================

        public bool InsuranceVerified { get; set; }

        public DateTime? InsuranceVerifiedAt { get; set; }

        public Guid? InsuranceVerifiedByUserId { get; set; }

        public string InsuranceVerifiedBy { get; set; } = "";
    }
}