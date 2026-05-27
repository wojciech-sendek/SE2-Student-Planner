using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.Schedule;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class ScheduleService : IScheduleService
    {
        private const string UniversityFacultyName = "university";

        private readonly ApplicationDbContext _dbContext;

        public ScheduleService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<EventDto>> GetScheduleAsync(
            string userId,
            IReadOnlyCollection<string> roles,
            DateTime? from = null,
            DateTime? to = null)
        {
            var personalQuery = _dbContext.PersonalEvents
                .AsNoTracking()
                .Where(e => e.UserId == userId);

            var usosQuery = _dbContext.UsosEvents
                .AsNoTracking()
                .Where(e => e.UserId == userId);

            var academicQuery = await BuildAcademicEventsQueryAsync(userId, roles);

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

            var academicEventsQuery = academicQuery.Select(e => new EventDto
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
                .Concat(academicEventsQuery)
                .OrderBy(e => e.StartTime)
                .ToListAsync();
        }

        private async Task<IQueryable<AcademicEvent>> BuildAcademicEventsQueryAsync(
            string userId,
            IReadOnlyCollection<string> roles)
        {
            var query = _dbContext.AcademicEvents.AsNoTracking();

            if (roles.Contains("Admin"))
            {
                return query;
            }

            if (roles.Contains("Manager"))
            {
                var facultyIds = await _dbContext.Users
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .SelectMany(u => u.Faculties.Select(f => f.Id))
                    .ToListAsync();

                return query.Where(e =>
                    facultyIds.Contains(e.FacultyId) ||
                    e.Faculty.Name == UniversityFacultyName);
            }

            return query.Where(e => e.Subscribers.Any(subscriber => subscriber.Id == userId));
        }
    }
}
