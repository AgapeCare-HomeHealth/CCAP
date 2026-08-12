namespace CCAP.Web.Features.Tracker.PatientWorkflow.Model
{
    public class KeyInformationDto
    {
        public string Coordinator { get; set; } = string.Empty;

        public string Clinician { get; set; } = string.Empty;

        public string Discipline { get; set; } = string.Empty;

        public int Episode { get; set; }

        public string Branch { get; set; } = string.Empty;

        public string Payor { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;
    }
}
