namespace CCAP.Web.Features.Tracker.PatientWorkflow.Models;

public class PatientHeaderDto
{
    public Guid PatientId { get; set; }

    public Guid ReferralId { get; set; }

    public string FirstName { get; set; } = "";

    public string MiddleName { get; set; } = "";

    public string LastName { get; set; } = "";

    public int Age { get; set; }

    public string MRN { get; set; } = "";

    public string ReferralNumber { get; set; } = "";

    public string Status { get; set; } = "";

    public DateOnly? SocDate { get; set; }

    public string Coordinator { get; set; } = "";

    public string Branch { get; set; } = "";

    public int EpisodeNumber { get; set; }

    public string FullName =>
     string.Join(" ",
         new[]
         {
            FirstName,
            MiddleName,
            LastName
         }.Where(x => !string.IsNullOrWhiteSpace(x)));

    public string Initials =>
        $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpper();
}