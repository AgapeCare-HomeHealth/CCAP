using CCAP.API.Contracts.Notifications;
using System.Security.Claims;
using CCAP.API.Authorization;
using CCAP.Application.Features.Notifications.Queries.GetNotifications;
using CCAP.Application.Features.Notifications.Commands.MarkNotificationRead;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CCAP.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize(Policy = PermissionPolicies.NotificationsView)]
public sealed class NotificationsController : ControllerBase
{
    private readonly ISender _sender;

    public NotificationsController(
        ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        CancellationToken cancellationToken)
    {
        var userIdValue =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
                userIdValue,
                out var userId))
        {
            return Unauthorized();
        }

        var result =
            await _sender.Send(
                new GetNotificationsQuery(userId),
                cancellationToken);

        return Ok(result);
    }
    [HttpPost("read")]
    public async Task<IActionResult> MarkRead([FromBody] MarkNotificationReadRequest request,CancellationToken cancellationToken)
    {
        var value=User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(!Guid.TryParse(value,out var userId)) return Unauthorized();
        if(request.NotificationId==Guid.Empty||string.IsNullOrWhiteSpace(request.NotificationType)) return BadRequest("NotificationId and NotificationType are required.");
        await _sender.Send(new MarkNotificationReadCommand(userId,request.NotificationId,request.NotificationType),cancellationToken);
        return NoContent();
    }


}