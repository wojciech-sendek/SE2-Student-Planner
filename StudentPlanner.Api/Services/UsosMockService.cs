using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.Usos;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class UsosMockService : IUsosMockService
    {
        private readonly ApplicationDbContext _dbContext;

        public UsosMockService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task EnsureConnectedAndSyncedAsync(string userId)
        {
            if (!await _dbContext.UsosTokens.AnyAsync(t => t.UserId == userId))
            {
                await ConnectAsync(userId);
                return;
            }

            if (!await _dbContext.UsosEvents.AnyAsync(e => e.UserId == userId))
            {
                await SyncAsync(userId);
            }
        }

        public async Task ConnectAsync(string userId)
        {
            var existing = await _dbContext.UsosTokens.FirstOrDefaultAsync(t => t.UserId == userId);
            if (existing is null)
            {
                _dbContext.UsosTokens.Add(new UsosToken
                {
                    UserId = userId,
                    AccessToken = $"mock-token-{userId}",
                    AccessTokenSecret = "mock-secret"
                });
                await _dbContext.SaveChangesAsync();
            }

            await SyncAsync(userId);
        }

        public async Task SyncAsync(string userId)
        {
            var existing = await _dbContext.UsosEvents.Where(e => e.UserId == userId).ToListAsync();
            if (existing.Count > 0)
            {
                _dbContext.UsosEvents.RemoveRange(existing);
            }

            var monday = GetMonday(DateTime.UtcNow.Date).AddDays(7);
            var sampleEvents = new List<UsosEvent>
            {
                new()
                {
                    UserId = userId,
                    Title = "Algorithms Lecture",
                    StartTime = monday.AddHours(8),
                    EndTime = monday.AddHours(9.5),
                    Location = "Room A1",
                    Room = "A1",
                    CourseId = "ALG101",
                    LecturerName = "Dr. Kowalski"
                },
                new()
                {
                    UserId = userId,
                    Title = "Databases Lab",
                    StartTime = monday.AddDays(1).AddHours(10),
                    EndTime = monday.AddDays(1).AddHours(11.5),
                    Location = "Lab B204",
                    Room = "B204",
                    CourseId = "DB202",
                    LecturerName = "Mgr. Nowak"
                },
                new()
                {
                    UserId = userId,
                    Title = "Software Engineering Project",
                    StartTime = monday.AddDays(2).AddHours(12),
                    EndTime = monday.AddDays(2).AddHours(13.5),
                    Location = "Room C12",
                    Room = "C12",
                    CourseId = "SE303",
                    LecturerName = "Dr. Wiśniewska"
                },
                new()
                {
                    UserId = userId,
                    Title = "Numerical Methods",
                    StartTime = monday.AddDays(4).AddHours(9),
                    EndTime = monday.AddDays(4).AddHours(10.5),
                    Location = "Room D7",
                    Room = "D7",
                    CourseId = "NM210",
                    LecturerName = "Dr. Zieliński"
                }
            };

            _dbContext.UsosEvents.AddRange(sampleEvents);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DisconnectAsync(string userId)
        {
            var tokens = await _dbContext.UsosTokens.Where(t => t.UserId == userId).ToListAsync();
            var events = await _dbContext.UsosEvents.Where(e => e.UserId == userId).ToListAsync();

            if (tokens.Count > 0) _dbContext.UsosTokens.RemoveRange(tokens);
            if (events.Count > 0) _dbContext.UsosEvents.RemoveRange(events);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<UsosStatusDto> GetStatusAsync(string userId)
        {
            return new UsosStatusDto
            {
                IsConnected = await _dbContext.UsosTokens.AnyAsync(t => t.UserId == userId),
                SyncedEventsCount = await _dbContext.UsosEvents.CountAsync(e => e.UserId == userId)
            };
        }

        public async Task<IReadOnlyList<AcademicEventDto>> GetAcademicEventsAsync(string userId, DateTime? from = null, DateTime? to = null)
        {
            var query = _dbContext.UsosEvents.AsNoTracking().Where(e => e.UserId == userId);

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
                .Select(e => new AcademicEventDto
                {
                    Id = e.Id,
                    Title = e.Title,
                    StartTime = e.StartTime,
                    EndTime = e.EndTime,
                    Location = e.Location,
                    CourseId = e.CourseId,
                    LecturerName = e.LecturerName,
                    Room = e.Room,
                    EventType = "Academic",
                    IsPersonal = false,
                    IsReadOnly = true
                })
                .ToListAsync();
        }

        private static DateTime GetMonday(DateTime date)
        {
            var diff = ((int)date.DayOfWeek + 6) % 7;
            return date.AddDays(-diff);
        }
    }
}
