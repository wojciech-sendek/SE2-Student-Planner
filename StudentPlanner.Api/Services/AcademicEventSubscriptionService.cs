using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.AcademicEvents;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class AcademicEventSubscriptionService : IAcademicEventSubscriptionService
    {
        private readonly ApplicationDbContext _dbContext;

        public AcademicEventSubscriptionService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<AcademicEventDto>> GetAvailableAsync(string userId, int? facultyId = null, DateTime? from = null, DateTime? to = null)
        {
            var query = ApplyDateFilters(_dbContext.AcademicEvents.AsNoTracking(), from, to);

            if (facultyId.HasValue)
            {
                query = query.Where(e => e.FacultyId == facultyId.Value);
            }

            return await ProjectToDto(query, userId)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<AcademicEventDto>> GetSubscribedAsync(string userId, DateTime? from = null, DateTime? to = null)
        {
            var query = ApplyDateFilters(_dbContext.AcademicEvents.AsNoTracking(), from, to)
                .Where(e => e.Subscribers.Any(subscriber => subscriber.Id == userId));

            return await ProjectToDto(query, userId)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        public async Task<AcademicEventDto?> SubscribeAsync(string userId, int academicEventId)
        {
            var academicEvent = await _dbContext.AcademicEvents
                .Include(e => e.Faculty)
                .Include(e => e.Subscribers)
                .FirstOrDefaultAsync(e => e.Id == academicEventId);

            if (academicEvent is null)
            {
                return null;
            }

            var user = await _dbContext.Users.FindAsync(userId)
                ?? throw new InvalidOperationException("Authenticated user was not found.");

            if (academicEvent.Subscribers.All(subscriber => subscriber.Id != userId))
            {
                academicEvent.Subscribers.Add(user);
                await _dbContext.SaveChangesAsync();
            }

            return ToDto(academicEvent, userId);
        }

        public async Task<AcademicEventDto?> UnsubscribeAsync(string userId, int academicEventId)
        {
            var academicEvent = await _dbContext.AcademicEvents
                .Include(e => e.Faculty)
                .Include(e => e.Subscribers)
                .FirstOrDefaultAsync(e => e.Id == academicEventId);

            if (academicEvent is null)
            {
                return null;
            }

            var subscriber = academicEvent.Subscribers.FirstOrDefault(u => u.Id == userId);
            if (subscriber is not null)
            {
                academicEvent.Subscribers.Remove(subscriber);
                await _dbContext.SaveChangesAsync();
            }

            return ToDto(academicEvent, userId);
        }

        private static IQueryable<AcademicEvent> ApplyDateFilters(IQueryable<AcademicEvent> query, DateTime? from, DateTime? to)
        {
            if (from.HasValue)
            {
                query = query.Where(e => e.EndTime >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(e => e.StartTime <= to.Value);
            }

            return query;
        }

        private static IQueryable<AcademicEventDto> ProjectToDto(IQueryable<AcademicEvent> query, string userId)
        {
            return query.Select(e => new AcademicEventDto
            {
                Id = e.Id,
                Title = e.Title,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = e.Location,
                FacultyId = e.FacultyId,
                FacultyName = e.Faculty.Name,
                FacultyDisplayName = e.Faculty.DisplayName,
                Source = "faculty",
                IsReadOnly = true,
                IsSubscribed = e.Subscribers.Any(subscriber => subscriber.Id == userId),
                SubscriberCount = e.Subscribers.Count
            });
        }

        private static AcademicEventDto ToDto(AcademicEvent academicEvent, string userId)
        {
            return new AcademicEventDto
            {
                Id = academicEvent.Id,
                Title = academicEvent.Title,
                StartTime = academicEvent.StartTime,
                EndTime = academicEvent.EndTime,
                Location = academicEvent.Location,
                FacultyId = academicEvent.FacultyId,
                FacultyName = academicEvent.Faculty.Name,
                FacultyDisplayName = academicEvent.Faculty.DisplayName,
                Source = "faculty",
                IsReadOnly = true,
                IsSubscribed = academicEvent.Subscribers.Any(subscriber => subscriber.Id == userId),
                SubscriberCount = academicEvent.Subscribers.Count
            };
        }
    }
}
