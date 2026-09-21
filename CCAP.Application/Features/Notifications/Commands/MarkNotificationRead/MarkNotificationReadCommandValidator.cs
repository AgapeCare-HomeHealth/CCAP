using CCAP.Application.Common.Validation;

namespace CCAP.Application.Features.Notifications.Commands.MarkNotificationRead;

public sealed class MarkNotificationReadCommandValidator : IRequestValidator<MarkNotificationReadCommand>
{
    public void Validate(MarkNotificationReadCommand request)
    {
        if (request.UserId == Guid.Empty) throw new ArgumentException("User ID is required.");
        if (request.NotificationId == Guid.Empty) throw new ArgumentException("Notification ID is required.");
        if (string.IsNullOrWhiteSpace(request.NotificationType)) throw new ArgumentException("Notification type is required.");
        if (request.NotificationType.Length > 50) throw new ArgumentException("Notification type cannot exceed 50 characters.");
    }
}
