using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace CCAP.Infrastructure.Persistence.Repositories;
public sealed class NotificationReadStateRepository:INotificationReadStateRepository
{ private readonly AppDbContext _context; public NotificationReadStateRepository(AppDbContext context)=>_context=context;
 public Task<bool> ExistsAsync(Guid userId,Guid notificationId,string notificationType,CancellationToken cancellationToken)=>_context.NotificationReadStates.AnyAsync(x=>x.UserId==userId&&x.NotificationId==notificationId&&x.NotificationType==notificationType,cancellationToken);
 public Task AddAsync(NotificationReadState state,CancellationToken cancellationToken)=>_context.NotificationReadStates.AddAsync(state,cancellationToken).AsTask(); }
