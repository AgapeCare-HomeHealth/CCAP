using CCAP.Application.Abstractions.Persistence;
using CCAP.Domain.Entities;
using MediatR;
namespace CCAP.Application.Features.Notifications.Commands.MarkNotificationRead;
public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand>
{
 private readonly INotificationReadStateRepository _readStates; private readonly IUnitOfWork _unitOfWork;
 public MarkNotificationReadCommandHandler(INotificationReadStateRepository readStates,IUnitOfWork unitOfWork){_readStates=readStates;_unitOfWork=unitOfWork;}
 public async Task Handle(MarkNotificationReadCommand request,CancellationToken cancellationToken){
  if(request.UserId==Guid.Empty||request.NotificationId==Guid.Empty) throw new ArgumentException("Valid user and notification are required.");
  var type=request.NotificationType.Trim(); if(string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Notification type is required.");
  if(await _readStates.ExistsAsync(request.UserId,request.NotificationId,type,cancellationToken)) return;
  await _readStates.AddAsync(new NotificationReadState(request.UserId,request.NotificationId,type),cancellationToken);
  await _unitOfWork.SaveChangesAsync(cancellationToken);
 }
}
