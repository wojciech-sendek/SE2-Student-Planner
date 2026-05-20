using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.Usos;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class MockUsosService : IUsosService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public MockUsosService(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public UsosAuthorizationUrlResponseDto CreateAuthorizationUrl(string userId)
        {
            return new UsosAuthorizationUrlResponseDto
            {
                AuthorizationUrl = null,
                State = null,
                Message = "Mock USOS mode is enabled. No external authorization is needed."
            };
        }

        public Task CompleteAuthorizationAsync(string code, string state)
        {
            return Task.FromException(new UsosApiException("Mock USOS mode does not use OAuth authorization."));
        }

        public async Task<IReadOnlyList<UsosEventDto>> GetScheduleAsync(string userId, DateTime? from = null, DateTime? to = null)
        {
            var query = _dbContext.UsosEvents
                .AsNoTracking()
                .Where(e => e.UserId == userId);

            if (from.HasValue)
            {
                query = query.Where(e => e.EndTime >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.StartTime <= to.Value);
            }

            return await query
                .OrderBy(e => e.StartTime)
                .Select(e => new UsosEventDto
                {
                    Id = e.Id,
                    ExternalId = e.ExternalId,
                    Title = e.Title,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Location = e.Location,
                    Room = e.Room,
                    Teacher = e.Teacher,
                    Source = "usos",
                    IsReadOnly = true,
                    SyncedAtUtc = e.SyncedAtUtc
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<UsosEventDto>> SyncScheduleForUserAsync(ApplicationUser user)
        {
            var today = DateTime.UtcNow.Date;
            var syncedAtUtc = DateTime.UtcNow;

            var mockEvents = new[]
            {
                new UsosEvent
                {
                    ExternalId = $"mock-{user.Id}-algorithms",
                    Title = "Algorithms",
                    StartTime = today.AddDays(1).AddHours(8),
                    EndTime = today.AddDays(1).AddHours(10),
                    Location = "Building A",
                    Room = "A-101",
                    Teacher = "Dr. Kowalski",
                    SyncedAtUtc = syncedAtUtc,
                    UserId = user.Id
                },
                new UsosEvent
                {
                    ExternalId = $"mock-{user.Id}-databases",
                    Title = "Databases",
                    StartTime = today.AddDays(2).AddHours(10),
                    EndTime = today.AddDays(2).AddHours(12),
                    Location = "Building C",
                    Room = "C-204",
                    Teacher = "Prof. Nowak",
                    SyncedAtUtc = syncedAtUtc,
                    UserId = user.Id
                },
                new UsosEvent
                {
                    ExternalId = $"mock-{user.Id}-software-engineering",
                    Title = "Software Engineering",
                    StartTime = today.AddDays(4).AddHours(12),
                    EndTime = today.AddDays(4).AddHours(14),
                    Location = "Building B",
                    Room = "B-12",
                    Teacher = "Mgr. Wisniewska",
                    SyncedAtUtc = syncedAtUtc,
                    UserId = user.Id
                }
            };

            var existingEvents = await _dbContext.UsosEvents
                .Where(e => e.UserId == user.Id)
                .ToListAsync();

            _dbContext.UsosEvents.RemoveRange(existingEvents);
            _dbContext.UsosEvents.AddRange(mockEvents);

            user.UsosRefreshTokenProtected ??= "MOCK-CONNECTED";
            user.UsosConnectedAtUtc ??= syncedAtUtc;
            user.UsosScheduleSyncedAtUtc = syncedAtUtc;

            await _userManager.UpdateAsync(user);
            await _dbContext.SaveChangesAsync();

            return await GetScheduleAsync(user.Id);
        }
    }
}
