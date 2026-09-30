using CCAP.Application.Features.Notifications.DTOs;
using MediatR;

namespace CCAP.Application.Features.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery(
    Guid UserId)
    : IRequest<IReadOnlyList<NotificationDto>>;