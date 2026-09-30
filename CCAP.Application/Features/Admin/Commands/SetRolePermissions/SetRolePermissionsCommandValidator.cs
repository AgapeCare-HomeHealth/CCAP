using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Admin.Commands.SetRolePermissions;

public sealed class SetRolePermissionsCommandValidator
    : IRequestValidator<SetRolePermissionsCommand>
{
    public void Validate(
        SetRolePermissionsCommand request)
    {
        if (request.RoleId == Guid.Empty)
        {
            throw new ArgumentException(
                "Role ID is required.");
        }

        if (request.PermissionIds is null)
        {
            throw new ArgumentException(
                "Permission IDs are required.");
        }

        if (request.PermissionIds.Any(
                id => id == Guid.Empty))
        {
            throw new ArgumentException(
                "Permission IDs cannot contain empty values.");
        }
    }
}