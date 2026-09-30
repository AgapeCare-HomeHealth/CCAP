using CCAP.Domain.Entities;
namespace CCAP.Application.Abstractions.Persistence;
public interface INotificationReadStateRepository
{ Task<bool> ExistsAsync(Guid userId,Guid notificationId,string notificationType,CancellationToken cancellationToken); Task AddAsync(NotificationReadState state,CancellationToken cancellationToken); }
