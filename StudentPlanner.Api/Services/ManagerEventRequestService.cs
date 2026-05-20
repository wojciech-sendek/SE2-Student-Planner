using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.AcademicEvents;
using StudentPlanner.Api.Dtos.EventRequests;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class ManagerEventRequestService : IManagerEventRequestService
    {
        private const string UniversityFacultyName = "university";
        private readonly ApplicationDbContext _dbContext;

        public ManagerEventRequestService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<AcademicEventDto>> GetVisibleAcademicEventsAsync(
            string managerId,
            DateTime? from = null,
            DateTime? to = null)
        {
            var managerFaculty = await GetManagerAssignedFacultyAsync(managerId);

            var query = _dbContext.AcademicEvents
                .AsNoTracking()
                .Include(e => e.Faculty)
                .Where(e => e.FacultyId == managerFaculty.Id || e.Faculty.Name == UniversityFacultyName);

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
                .ThenBy(e => e.Title)
                .Select(e => ToAcademicEventDto(e))
                .ToListAsync();
        }

        public async Task<IReadOnlyList<EventRequestDto>> GetMineAsync(string managerId)
        {
            return await _dbContext.EventRequests
                .AsNoTracking()
                .Include(r => r.Faculty)
                .Include(r => r.Manager)
                .Where(r => r.ManagerId == managerId)
                .OrderByDescending(r => r.CreatedAtUtc)
                .Select(r => ToEventRequestDto(r))
                .ToListAsync();
        }

        public async Task<EventRequestDto?> GetMineByIdAsync(string managerId, int requestId)
        {
            return await _dbContext.EventRequests
                .AsNoTracking()
                .Include(r => r.Faculty)
                .Include(r => r.Manager)
                .Where(r => r.Id == requestId && r.ManagerId == managerId)
                .Select(r => ToEventRequestDto(r))
                .FirstOrDefaultAsync();
        }

        public async Task<EventRequestDto> SubmitCreateRequestAsync(string managerId, CreateEventRequestDto dto)
        {
            var managerFaculty = await GetManagerAssignedFacultyAsync(managerId);

            if (dto.FacultyId.HasValue && dto.FacultyId.Value != managerFaculty.Id)
            {
                throw new ManagerFacultyAccessDeniedException();
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var request = new EventRequest
                {
                    RequestType = EventRequestType.Create,
                    Status = EventRequestStatus.Pending,
                    Title = dto.Title.Trim(),
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    Location = NormalizeOptional(dto.Location),
                    FacultyId = managerFaculty.Id,
                    ManagerId = managerId,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _dbContext.EventRequests.Add(request);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetMineByIdAsync(managerId, request.Id))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<EventRequestDto> SubmitUpdateRequestAsync(string managerId, int academicEventId, UpdateEventRequestDto dto)
        {
            var managerFaculty = await GetManagerAssignedFacultyAsync(managerId);
            var targetEvent = await GetTargetEventForManagerAsync(academicEventId, managerFaculty.Id);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var request = new EventRequest
                {
                    RequestType = EventRequestType.Update,
                    Status = EventRequestStatus.Pending,
                    TargetAcademicEventId = targetEvent.Id,
                    Title = dto.Title.Trim(),
                    StartTime = dto.StartTime,
                    EndTime = dto.EndTime,
                    Location = NormalizeOptional(dto.Location),
                    FacultyId = managerFaculty.Id,
                    ManagerId = managerId,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _dbContext.EventRequests.Add(request);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetMineByIdAsync(managerId, request.Id))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<EventRequestDto> SubmitDeleteRequestAsync(string managerId, int academicEventId, DeleteEventRequestDto? dto = null)
        {
            var managerFaculty = await GetManagerAssignedFacultyAsync(managerId);
            var targetEvent = await GetTargetEventForManagerAsync(academicEventId, managerFaculty.Id);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var request = new EventRequest
                {
                    RequestType = EventRequestType.Delete,
                    Status = EventRequestStatus.Pending,
                    TargetAcademicEventId = targetEvent.Id,
                    Title = targetEvent.Title,
                    StartTime = targetEvent.StartTime,
                    EndTime = targetEvent.EndTime,
                    Location = targetEvent.Location,
                    FacultyId = managerFaculty.Id,
                    ManagerId = managerId,
                    CreatedAtUtc = DateTime.UtcNow,
                    ReviewComment = NormalizeOptional(dto?.Reason)
                };

                _dbContext.EventRequests.Add(request);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (await GetMineByIdAsync(managerId, request.Id))!;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<Faculty> GetManagerAssignedFacultyAsync(string managerId)
        {
            var manager = await _dbContext.Users
                .Include(u => u.Faculties)
                .FirstOrDefaultAsync(u => u.Id == managerId);

            var faculty = manager?.Faculties
                .Where(f => f.Name != UniversityFacultyName)
                .OrderBy(f => f.Id)
                .FirstOrDefault();

            return faculty ?? throw new ManagerFacultyNotAssignedException();
        }

        private async Task<AcademicEvent> GetTargetEventForManagerAsync(int academicEventId, int managerFacultyId)
        {
            var targetEvent = await _dbContext.AcademicEvents
                .AsNoTracking()
                .Include(e => e.Faculty)
                .FirstOrDefaultAsync(e => e.Id == academicEventId);

            if (targetEvent is null)
            {
                throw new AcademicEventNotFoundException(academicEventId);
            }

            if (targetEvent.FacultyId != managerFacultyId && targetEvent.Faculty.Name != UniversityFacultyName)
            {
                throw new ManagerFacultyAccessDeniedException();
            }

            return targetEvent;
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static AcademicEventDto ToAcademicEventDto(AcademicEvent academicEvent)
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
                FacultyDisplayName = academicEvent.Faculty.DisplayName
            };
        }

        private static EventRequestDto ToEventRequestDto(EventRequest request)
        {
            return new EventRequestDto
            {
                Id = request.Id,
                RequestType = request.RequestType.ToString(),
                Status = request.Status.ToString(),
                TargetAcademicEventId = request.TargetAcademicEventId,
                Title = request.Title,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Location = request.Location,
                FacultyId = request.FacultyId,
                FacultyName = request.Faculty.Name,
                FacultyDisplayName = request.Faculty.DisplayName,
                ManagerId = request.ManagerId,
                ManagerEmail = request.Manager.Email,
                CreatedAtUtc = request.CreatedAtUtc,
                ReviewedAtUtc = request.ReviewedAtUtc,
                ReviewComment = request.ReviewComment
            };
        }
    }
}
