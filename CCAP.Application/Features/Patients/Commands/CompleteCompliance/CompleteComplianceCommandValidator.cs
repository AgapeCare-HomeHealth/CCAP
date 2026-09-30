using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Patients.Commands.CompleteCompliance;

public sealed class CompleteComplianceCommandValidator
    : IRequestValidator<CompleteComplianceCommand>
{
    public void Validate(
        CompleteComplianceCommand request)
    {
        if (request.PatientId == Guid.Empty)
        {
            throw new ArgumentException(
                "Patient ID is required.");
        }

        if (string.IsNullOrWhiteSpace(
            request.RequirementCode))
        {
            throw new ArgumentException(
                "Compliance requirement is required.");
        }

        if (request.CompletedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Completed by user ID is required.");
        }
    }
}