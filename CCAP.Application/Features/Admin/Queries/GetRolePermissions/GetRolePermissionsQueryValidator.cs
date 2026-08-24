using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Admin.Queries.GetRolePermissions;

public sealed class GetRolePermissionsQueryValidator
    : IRequestValidator<GetRolePermissionsQuery>
{
    public void Validate(
        GetRolePermissionsQuery request)
    {
        if (request.RoleId == Guid.Empty)
        {
            throw new ArgumentException(
                "Role ID is required.");
        }
    }
}