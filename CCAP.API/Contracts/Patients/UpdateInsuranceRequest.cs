namespace CCAP.API.Contracts.Patients;

public sealed class UpdateInsuranceRequest
{
    public string? PrimaryInsurance { get; set; }

    public string? InsuranceMemberId { get; set; }

    public DateOnly? AuthorizationDate { get; set; }

    public int? ApprovedVisits { get; set; }

    public bool AuthorizationRequired { get; set; }
}