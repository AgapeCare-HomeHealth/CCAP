using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Users.Commands.DeleteUser;

public sealed class DeleteUserCommandValidator
    : IRequestValidator<DeleteUserCommand>
{
    public void Validate(
        DeleteUserCommand request)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.");
        }
    }
}