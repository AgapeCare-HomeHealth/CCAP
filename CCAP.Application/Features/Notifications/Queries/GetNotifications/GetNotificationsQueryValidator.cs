using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsQueryValidator
    : IRequestValidator<GetNotificationsQuery>
{
    public void Validate(
        GetNotificationsQuery request)
    {
        if (request.UserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.");
        }
    }
}