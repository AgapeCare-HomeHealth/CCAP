namespace CCAP.API.Contracts.Notifications;

public sealed record MarkNotificationReadRequest(Guid NotificationId, string NotificationType);
