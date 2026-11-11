using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
namespace CCAP.API.Hubs;
[Authorize]
public sealed class NotificationsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub") ?? Context.User?.FindFirstValue("userId");
        if (Guid.TryParse(userId, out var id))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{id}");
        await base.OnConnectedAsync();
    }
}
