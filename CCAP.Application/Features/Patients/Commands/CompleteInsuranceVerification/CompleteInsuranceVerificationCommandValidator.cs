using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Patients.Commands.CompleteInsuranceVerification;

public sealed class CompleteInsuranceVerificationCommandValidator : IRequestValidator<CompleteInsuranceVerificationCommand>
{
    public void Validate(CompleteInsuranceVerificationCommand request)
    {
        if (request.PatientId == Guid.Empty) throw new ArgumentException("Patient ID is required.");
        if (request.VerifiedByUserId == Guid.Empty) throw new ArgumentException("Verified by user ID is required.");
    }
}
