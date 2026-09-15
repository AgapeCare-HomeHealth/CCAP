using MediatR;
namespace CCAP.Application.Features.Notifications.Commands.MarkNotificationRead;
public sealed record MarkNotificationReadCommand(Guid UserId, Guid NotificationId, string NotificationType) : IRequest;
