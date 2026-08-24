using CCAP.Application.Abstractions.Persistence;
using CCAP.Application.Features.Notifications.DTOs;
using MediatR;

namespace CCAP.Application.Features.Notifications.Queries.GetNotifications;

public sealed class GetNotificationsQueryHandler
    : IRequestHandler<
        GetNotificationsQuery,
        IReadOnlyList<NotificationDto>>
{
    private readonly INotificationRepository _repository;

    public GetNotificationsQueryHandler(
        INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        return await _repository.GetForUserAsync(
            request.UserId,
            cancellationToken);
    }
}