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

            if (from.HasValue)
            {
                personalQuery = personalQuery.Where(e => e.EndTime >= from.Value);
                usosQuery = usosQuery.Where(e => e.EndTime >= from.Value);
            }

            if (to.HasValue)
            {
                personalQuery = personalQuery.Where(e => e.StartTime <= to.Value);
                usosQuery = usosQuery.Where(e => e.StartTime <= to.Value);
            }

            var personalEvents = await personalQuery
                .Select(e => new EventDto
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
                })
                .ToListAsync();

            var usosEvents = await usosQuery
                .Select(e => new EventDto
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
                })
                .ToListAsync();

            return personalEvents.Concat(usosEvents)
                .OrderBy(e => e.StartTime)
                .ToList();
        }
    }
}