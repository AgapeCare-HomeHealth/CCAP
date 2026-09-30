using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Admin.Commands.DeleteRole;

public sealed class DeleteRoleCommandValidator
    : IRequestValidator<DeleteRoleCommand>
{
    public void Validate(
        DeleteRoleCommand request)
    {
        if (request.RoleId == Guid.Empty)
        {
            throw new ArgumentException(
                "Role ID is required.");
        }
    }
}