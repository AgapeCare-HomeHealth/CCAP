using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Admin.Commands.UpdateRole;

public sealed class UpdateRoleCommandValidator
    : IRequestValidator<UpdateRoleCommand>
{
    public void Validate(
        UpdateRoleCommand request)
    {
        if (request.RoleId == Guid.Empty)
        {
            throw new ArgumentException(
                "Role ID is required.");
        }

        if (string.IsNullOrWhiteSpace(request.RoleName))
        {
            throw new ArgumentException(
                "Role name is required.");
        }

        if (request.RoleName.Length > 100)
        {
            throw new ArgumentException(
                "Role name cannot exceed 100 characters.");
        }

        if (!string.IsNullOrWhiteSpace(request.Description) &&
            request.Description.Length > 500)
        {
            throw new ArgumentException(
                "Role description cannot exceed 500 characters.");
        }
    }
}