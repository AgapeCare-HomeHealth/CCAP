namespace CCAP.Domain.Entities;
public sealed class NotificationReadState
{
 private NotificationReadState(){}
 public Guid NotificationReadStateId{get;private set;}
 public Guid UserId{get;private set;}
 public Guid NotificationId{get;private set;}
 public string NotificationType{get;private set;}=string.Empty;
 public DateTime ReadAt{get;private set;}
 public NotificationReadState(Guid userId,Guid notificationId,string notificationType)
 {
  if(userId==Guid.Empty)throw new ArgumentException("User is required.",nameof(userId));
  if(notificationId==Guid.Empty)throw new ArgumentException("Notification is required.",nameof(notificationId));
  if(string.IsNullOrWhiteSpace(notificationType))throw new ArgumentException("Notification type is required.",nameof(notificationType));
  NotificationReadStateId=Guid.NewGuid();UserId=userId;NotificationId=notificationId;NotificationType=notificationType.Trim();ReadAt=DateTime.UtcNow;
 }
}
