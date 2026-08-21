namespace CCAP.Web.Features.Tracker.ReferralIntake.Models;

public sealed class ReferralIntakeResultDto
{
    public Guid PatientId { get; set; }

    public Guid ReferralId { get; set; }

    public string ReferralNumber { get; set; }
        = string.Empty;

    public Guid ReferralDocumentId { get; set; }

    public string StorageKey { get; set; }
        = string.Empty;
}