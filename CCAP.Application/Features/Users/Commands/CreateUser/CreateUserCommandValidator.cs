using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator
    : IRequestValidator<CreateUserCommand>
{
    public void Validate(CreateUserCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.EmployeeNo))
        {
            throw new ArgumentException(
                "Employee number is required.");
        }

        if (request.EmployeeNo.Length > 50)
        {
            throw new ArgumentException(
                "Employee number cannot exceed 50 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw new ArgumentException(
                "First name is required.");
        }

        if (request.FirstName.Length > 100)
        {
            throw new ArgumentException(
                "First name cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new ArgumentException(
                "Last name is required.");
        }

        if (request.LastName.Length > 100)
        {
            throw new ArgumentException(
                "Last name cannot exceed 100 characters.");
        }

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

        if (request.Password.Length < 8)
        {
            throw new ArgumentException(
                "Password must be at least 8 characters.");
        }

        if (request.Password.Length > 200)
        {
            throw new ArgumentException(
                "Password cannot exceed 200 characters.");
        }

        if (request.RoleId == Guid.Empty)
        {
            throw new ArgumentException(
                "Role is required.");
        }

        if (request.DisciplineId.HasValue &&
            request.DisciplineId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Discipline ID is invalid.");
        }

        if (!string.IsNullOrWhiteSpace(request.MobileNo) &&
            request.MobileNo.Length > 30)
        {
            throw new ArgumentException(
                "Mobile number cannot exceed 30 characters.");
        }
    }
}