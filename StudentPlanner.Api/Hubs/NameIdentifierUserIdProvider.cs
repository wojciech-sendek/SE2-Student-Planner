using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace StudentPlanner.Api.Hubs
{
    public class NameIdentifierUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
