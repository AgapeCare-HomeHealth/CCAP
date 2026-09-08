using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Patients.Commands.CompleteSocCompliance;

public sealed class CompleteSocComplianceCommandValidator
    : IRequestValidator<CompleteSocComplianceCommand>
{
    public void Validate(
        CompleteSocComplianceCommand request)
    {
        if (request.PatientId == Guid.Empty)
        {
            throw new ArgumentException(
                "Patient ID is required.");
        }

        if (request.VerifiedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Verified by user ID is required.");
        }
    }
}