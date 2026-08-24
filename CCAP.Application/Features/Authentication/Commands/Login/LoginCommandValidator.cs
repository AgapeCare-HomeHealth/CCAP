using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Authentication.Commands.Login;

public sealed class LoginCommandValidator
    : IRequestValidator<LoginCommand>
{
    public void Validate(
        LoginCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new ArgumentException(
                "Email is required.");
        }

        if (request.Email.Length > 320)
        {
            throw new ArgumentException(
                "Email cannot exceed 320 characters.");
        }

        if (!request.Email.Contains('@'))
        {
            throw new ArgumentException(
                "A valid email address is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException(
                "Password is required.");
        }
    }
}