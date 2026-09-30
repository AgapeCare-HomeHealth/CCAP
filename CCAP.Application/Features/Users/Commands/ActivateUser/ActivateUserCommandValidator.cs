using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Users.Commands.ActivateUser;

public sealed class ActivateUserCommandValidator
    : IRequestValidator<ActivateUserCommand>
{
    public void Validate(
        ActivateUserCommand request)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.");
        }
    }
}