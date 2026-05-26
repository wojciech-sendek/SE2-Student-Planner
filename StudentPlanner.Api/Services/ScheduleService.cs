using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.Schedule;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class ScheduleService : IScheduleService
    {
        private readonly ApplicationDbContext _dbContext;

        public ScheduleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<EventDto>> GetScheduleAsync(string userId, DateTime? from = null, DateTime? to = null)
        {
            var personalQuery = _dbContext.PersonalEvents
                .AsNoTracking()
                .Where(e => e.UserId == userId);

            var usosQuery = _dbContext.UsosEvents
                .AsNoTracking()
                .Where(e => e.UserId == userId);

            var academicQuery = _dbContext.AcademicEvents
                .AsNoTracking()
                .Where(e => e.Subscribers.Any(subscriber => subscriber.Id == userId));

            if (from.HasValue)
            {
                personalQuery = personalQuery.Where(e => e.EndTime >= from.Value);
                usosQuery = usosQuery.Where(e => e.EndTime >= from.Value);
                academicQuery = academicQuery.Where(e => e.EndTime >= from.Value);
            }

            if (to.HasValue)
            {
                personalQuery = personalQuery.Where(e => e.StartTime <= to.Value);
                usosQuery = usosQuery.Where(e => e.StartTime <= to.Value);
                academicQuery = academicQuery.Where(e => e.StartTime <= to.Value);
            }

            var personalEventsQuery = personalQuery.Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = e.Location,
                EventType = "personal",
                IsPersonal = true,
                Room = null,
                Teacher = null
            });

            var usosEventsQuery = usosQuery.Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = e.Location,
                EventType = "usos",
                IsPersonal = false,
                Room = e.Room,
                Teacher = e.Teacher
            });

            var subscribedAcademicEventsQuery = academicQuery.Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                Location = e.Location,
                EventType = "academic",
                IsPersonal = false,
                Room = null,
                Teacher = null
            });

            return await personalEventsQuery
                .Concat(usosEventsQuery)
                .Concat(subscribedAcademicEventsQuery)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }
    }
}