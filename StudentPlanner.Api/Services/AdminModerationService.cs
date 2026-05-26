using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.EventRequests;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class AdminModerationService : IAdminModerationService
    {
        private readonly ApplicationDbContext _dbContext;

        public AdminModerationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<EventRequestDto>> GetEventRequestsAsync(EventRequestStatus? status = null, int? facultyId = null)
        {
            var query = _dbContext.EventRequests
                .AsNoTracking()
                .Include(r => r.Faculty)
                .Include(r => r.Manager)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(r => r.Status == status.Value);
            }

            if (facultyId.HasValue)
            {
                query = query.Where(r => r.FacultyId == facultyId.Value);
            }

            var requests = await query
                .OrderBy(r => r.Status == EventRequestStatus.Pending ? 0 : 1)
                .ThenByDescending(r => r.CreatedAtUtc)
                .ToListAsync();

            return requests.Select(ToEventRequestDto).ToList();
        }

        public async Task<EventRequestDto?> GetEventRequestAsync(int requestId)
        {
            var request = await _dbContext.EventRequests
                .AsNoTracking()
                .Include(r => r.Faculty)
                .Include(r => r.Manager)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            return request is null ? null : ToEventRequestDto(request);
        }

        public async Task<EventRequestDto> ReviewEventRequestAsync(string adminId, int requestId, EventRequestStatus newStatus, string? reviewComment = null)
        {
            if (newStatus is not EventRequestStatus.Approved and not EventRequestStatus.Rejected)
            {
                throw new InvalidOperationException("Event request can only be approved or rejected.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var request = await _dbContext.EventRequests
                .Include(r => r.Faculty)
                .Include(r => r.Manager)
                .Include(r => r.TargetAcademicEvent)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request is null)
            {
                throw new KeyNotFoundException("Event request was not found.");
            }

            if (request.Status != EventRequestStatus.Pending)
            {
                throw new InvalidOperationException("Only pending event requests can be reviewed.");
            }

            request.Status = newStatus;
            request.AdminId = adminId;
            request.ReviewedAtUtc = DateTime.UtcNow;
            request.ReviewComment = NormalizeOptional(reviewComment);

            if (newStatus == EventRequestStatus.Approved)
            {
                await ApplyApprovedRequestAsync(request);
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return ToEventRequestDto(request);
        }

        private async Task ApplyApprovedRequestAsync(EventRequest request)
        {
            switch (request.RequestType)
            {
                case EventRequestType.Create:
                    request.TargetAcademicEvent = new AcademicEvent
                    {
                        Title = request.Title,
                        StartTime = request.StartTime,
                        EndTime = request.EndTime,
                        Location = request.Location,
                        FacultyId = request.FacultyId
                    };
                    break;

                case EventRequestType.Update:
                    if (request.TargetAcademicEvent is null)
                    {
                        throw new InvalidOperationException("Target academic event was not found for update request.");
                    }

                    request.TargetAcademicEvent.Title = request.Title;
                    request.TargetAcademicEvent.StartTime = request.StartTime;
                    request.TargetAcademicEvent.EndTime = request.EndTime;
                    request.TargetAcademicEvent.Location = request.Location;
                    request.TargetAcademicEvent.FacultyId = request.FacultyId;
                    break;

                case EventRequestType.Delete:
                    if (request.TargetAcademicEvent is null)
                    {
                        throw new InvalidOperationException("Target academic event was not found for delete request.");
                    }

                    var target = request.TargetAcademicEvent;
                    var targetEventId = target.Id;

                    var requestsPointingToDeletedEvent = await _dbContext.EventRequests
                        .Where(r => r.TargetAcademicEventId == targetEventId)
                        .ToListAsync();

                    foreach (var relatedRequest in requestsPointingToDeletedEvent)
                    {
                        relatedRequest.TargetAcademicEvent = null;
                        relatedRequest.TargetAcademicEventId = null;
                    }

                    // Flush FK nulling first, otherwise SQL Server blocks deleting AcademicEvent.
                    await _dbContext.SaveChangesAsync();

                    _dbContext.AcademicEvents.Remove(target);
                    break;

                default:
                    throw new InvalidOperationException("Unsupported event request type.");
            }
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
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
