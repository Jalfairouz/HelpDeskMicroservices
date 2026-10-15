using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace NotificationService.Hubs;

[Authorize]
public sealed class NotificationHub : Hub
{
}