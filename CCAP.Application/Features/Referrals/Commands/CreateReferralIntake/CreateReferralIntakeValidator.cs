using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Referrals.Commands.CreateReferralIntake;

public sealed class CreateReferralIntakeValidator
    : IRequestValidator<CreateReferralIntakeCommand>
{
    public void Validate(CreateReferralIntakeCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) &&
            string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ArgumentException("Patient name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.FirstName))
            throw new ArgumentException("First name is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            throw new ArgumentException("Last name is required.");

        if (request.DateOfBirth is null)
            throw new ArgumentException("Date of birth is required.");
    }
}
