namespace CCAP.Application.Features.Referrals.Commands.CreateReferralIntake;

public static class CreateReferralIntakeValidator
{
    public static void Validate(
        CreateReferralIntakeCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.MRN))
            throw new ArgumentException(
                "MRN is required.");

        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new ArgumentException(
                "First name is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new ArgumentException(
                "Last name is required.");

        if (string.IsNullOrWhiteSpace(request.ReferralNumber))
            throw new ArgumentException(
                "Referral number is required.");

        if (request.ReferralDate.Date > DateTime.UtcNow.Date)
            throw new ArgumentException(
                "Referral date cannot be in the future.");

        if (request.PdfStream is null)
            throw new ArgumentException(
                "Referral PDF is required.");

        if (string.IsNullOrWhiteSpace(request.PdfFileName))
            throw new ArgumentException(
                "Referral PDF file name is required.");

        if (!request.PdfFileName
                .EndsWith(
                    ".pdf",
                    StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Only PDF files are accepted.");
        }

        if (request.CoordinatorId is null)
            throw new ArgumentException(
                "Care Coordinator is required.");

        if (request.DisciplineId is null)
            throw new ArgumentException(
                "Discipline is required.");

        if (request.SocDate is null)
            throw new ArgumentException(
                "SOC date is required.");

        if (string.IsNullOrWhiteSpace(request.CaseStatus))
            throw new ArgumentException(
                "Case status is required.");
    }
}