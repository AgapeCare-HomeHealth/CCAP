using System.ComponentModel.DataAnnotations;

namespace CCAP.Web.Features.Tracker.PatientWorkflow.Models;

public sealed class PatientEditModel
{
    public Guid PatientId { get; set; }
    [Required, StringLength(50)] public string MRN { get; set; } = string.Empty;
    public string ReferralNumber { get; set; } = string.Empty;
    [Required, StringLength(100)] public string FirstName { get; set; } = string.Empty;
    [StringLength(100)] public string MiddleName { get; set; } = string.Empty;
    [Required, StringLength(100)] public string LastName { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateOnly? SocDate { get; set; }
    public string Coordinator { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string PrimaryPhone { get; set; } = string.Empty;
    public string AlternatePhone { get; set; } = string.Empty;
    public string StreetAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactRelationship { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public DateTime? ReferralDate { get; set; }
    public string ReferralType { get; set; } = string.Empty;
    public string ReferralSource { get; set; } = string.Empty;
    public string ReferralChannel { get; set; } = string.Empty;
    public string Priority { get; set; } = "Routine";
    public string ReferringPhysician { get; set; } = string.Empty;
    public string PhysicianPhone { get; set; } = string.Empty;
    public string PhysicianFax { get; set; } = string.Empty;
    public string PhysicianNpi { get; set; } = string.Empty;
    public string PcosRegistered { get; set; } = string.Empty;
    public string PrimaryDiagnosis { get; set; } = string.Empty;
    public string SecondaryDiagnosis { get; set; } = string.Empty;
    public string OrderedServicesText { get; set; } = string.Empty;
    public string ReferralNotes { get; set; } = string.Empty;
    public string VisitPriority { get; set; } = string.Empty;
    public string CaseStatus { get; set; } = string.Empty;
    public string InternalNotes { get; set; } = string.Empty;
}
