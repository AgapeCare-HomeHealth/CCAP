using CCAP.Application.Common.Validation;
using System.Net.Mail;

namespace CCAP.Application.Features.Authentication.Commands.Login;

public sealed class LoginCommandValidator
    : IRequestValidator<LoginCommand>
{
    public void Validate(LoginCommand request)
    {
        var errors = new List<string>();

        var email = request.Email?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            errors.Add("Email is required.");
        }
        else
        {
            if (email.Length > 320)
            {
                errors.Add("Email cannot exceed 320 characters.");
            }

            if (!IsValidEmail(email))
            {
                errors.Add("Enter a valid email address.");
            }
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add("Password is required.");
        }
        else
        {
            if (request.Password.Length < 8)
            {
                errors.Add("Password must be at least 8 characters.");
            }

            if (request.Password.Length > 200)
            {
                errors.Add("Password cannot exceed 200 characters.");
            }
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var parsed = new MailAddress(email);
            return string.Equals(parsed.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
