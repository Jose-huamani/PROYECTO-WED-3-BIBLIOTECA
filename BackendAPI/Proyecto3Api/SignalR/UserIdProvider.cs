using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace Proyecto3Api.SignalR;

public sealed class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
