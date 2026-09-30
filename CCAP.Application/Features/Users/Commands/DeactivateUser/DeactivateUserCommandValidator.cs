using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Users.Commands.DeactivateUser;

public sealed class DeactivateUserCommandValidator
    : IRequestValidator<DeactivateUserCommand>
{
    public void Validate(
        DeactivateUserCommand request)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.");
        }
    }
}