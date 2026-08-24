using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Dashboard.Queries.GetDashboard;

public sealed class GetDashboardQueryValidator
    : IRequestValidator<GetDashboardQuery>
{
    public void Validate(
        GetDashboardQuery request)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.");
        }
    }
}