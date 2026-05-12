using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Proyecto3Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
}
