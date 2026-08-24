using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Patients.Commands.CompleteCare;

public sealed class CompleteCareCommandValidator
    : IRequestValidator<CompleteCareCommand>
{
    public void Validate(
        CompleteCareCommand request)
    {
        if (request.PatientId == Guid.Empty)
        {
            throw new ArgumentException(
                "Patient ID is required.");
        }

        if (request.FinalizedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "Finalized by user ID is required.");
        }

        if (string.IsNullOrWhiteSpace(request.FinalStatus))
        {
            throw new ArgumentException(
                "Final status is required.");
        }

        if (request.FinalStatus.Length > 100)
        {
            throw new ArgumentException(
                "Final status cannot exceed 100 characters.");
        }
    }
}