namespace CCAP.Web.Features.Tracker.PatientWorkflow.Models;

public sealed class InsuranceEditModel
{
    public string PrimaryInsurance { get; set; } = string.Empty;

    public string InsuranceMemberId { get; set; } = string.Empty;

    public DateOnly? AuthorizationDate { get; set; }

    public int? ApprovedVisits { get; set; }

    public bool AuthorizationRequired { get; set; }
}