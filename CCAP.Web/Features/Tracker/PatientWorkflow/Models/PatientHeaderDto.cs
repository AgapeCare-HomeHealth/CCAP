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
    public DateOnly? DateOfBirth { get; set; }
    public string Gender { get; set; } = "";
    public string PrimaryPhone { get; set; } = "";
    public string AlternatePhone { get; set; } = "";
    public string StreetAddress { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";
    public string EmergencyContactName { get; set; } = "";
    public string EmergencyContactRelationship { get; set; } = "";
    public string EmergencyContactPhone { get; set; } = "";
    public DateTime? ReferralDate { get; set; }
    public string ReferralType { get; set; } = "";
    public string ReferralSource { get; set; } = "";
    public string ReferralChannel { get; set; } = "";
    public string Priority { get; set; } = "Routine";
    public string ReferringPhysician { get; set; } = "";
    public string PhysicianPhone { get; set; } = "";
    public string PhysicianFax { get; set; } = "";
    public string PhysicianNpi { get; set; } = "";
    public string PcosRegistered { get; set; } = "";
    public string PrimaryDiagnosis { get; set; } = "";
    public string SecondaryDiagnosis { get; set; } = "";
    public string ReferralNotes { get; set; } = "";
    public string VisitPriority { get; set; } = "";
    public string CaseStatus { get; set; } = "";
    public string InternalNotes { get; set; } = "";

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