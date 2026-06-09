using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Data;

namespace StudentPlanner.Api.Hubs
{
    [Authorize]
    public class NotificationsHub : Hub
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<NotificationsHub> _logger;

        public NotificationsHub(ApplicationDbContext dbContext, ILogger<NotificationsHub> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetCurrentUserId();

            if (userId is not null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.User(userId));

                foreach (var role in GetCurrentRoles())
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.Role(role));
                }

                var facultyIds = await _dbContext.Users
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .SelectMany(u => u.Faculties.Select(f => f.Id))
                    .ToListAsync();

                foreach (var facultyId in facultyIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.Faculty(facultyId));
                }

                var subscribedAcademicEventIds = await _dbContext.AcademicEvents
                    .AsNoTracking()
                    .Where(e => e.Subscribers.Any(subscriber => subscriber.Id == userId))
                    .Select(e => e.Id)
                    .ToListAsync();

                foreach (var academicEventId in subscribedAcademicEventIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.AcademicEvent(academicEventId));
                }
            }

            await Clients.Caller.SendAsync("Connected", new
            {
                Context.ConnectionId,
                UserId = userId,
                ConnectedAtUtc = DateTime.UtcNow
            });

            await base.OnConnectedAsync();
        }

        public async Task JoinAcademicEventGroup(int academicEventId)
        {
            var userId = GetCurrentUserIdOrThrow();
            var isPrivileged = Context.User?.IsInRole("Admin") == true || Context.User?.IsInRole("Manager") == true;

            var hasAccess = isPrivileged || await _dbContext.AcademicEvents
                .AsNoTracking()
                .AnyAsync(e => e.Id == academicEventId && e.Subscribers.Any(subscriber => subscriber.Id == userId));

            if (!hasAccess)
            {
                _logger.LogWarning(
                    "User {UserId} attempted to join academic event SignalR group {AcademicEventId} without access.",
                    userId,
                    academicEventId);

                throw new HubException("You are not subscribed to this academic event.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.AcademicEvent(academicEventId));
        }

        public async Task LeaveAcademicEventGroup(int academicEventId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, RealtimeGroups.AcademicEvent(academicEventId));
        }

        private string? GetCurrentUserId()
        {
            var userId = Context.UserIdentifier ?? Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrWhiteSpace(userId) ? null : userId;
        }

        private string GetCurrentUserIdOrThrow()
        {
            return GetCurrentUserId() ?? throw new HubException("Authenticated user id was not found.");
        }

        private IEnumerable<string> GetCurrentRoles()
        {
            return Context.User?.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                ?? Enumerable.Empty<string>();
        }
    }
}
