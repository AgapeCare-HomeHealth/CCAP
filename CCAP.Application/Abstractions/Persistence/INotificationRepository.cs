using CCAP.Application.Features.Notifications.DTOs;

namespace CCAP.Application.Abstractions.Persistence;

public interface INotificationRepository
{
    Task<IReadOnlyList<NotificationDto>> GetForUserAsync(
        Guid userId,
        CancellationToken cancellationToken);
}