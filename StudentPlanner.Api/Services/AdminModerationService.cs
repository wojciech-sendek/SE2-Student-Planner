using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.EventRequests;
using StudentPlanner.Api.Dtos.Notifications;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class AdminModerationService : IAdminModerationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IRealtimeNotificationService _realtimeNotificationService;
        private readonly ILogger<AdminModerationService> _logger;

        public AdminModerationService(
            ApplicationDbContext dbContext,
            IRealtimeNotificationService realtimeNotificationService,
            ILogger<AdminModerationService> logger)
        {
            _dbContext = dbContext;
            _realtimeNotificationService = realtimeNotificationService;
            _logger = logger;
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

        public async Task<EventRequestDto> ReviewEventRequestAsync(
            string adminId,
            int requestId,
            EventRequestStatus newStatus,
            string? reviewComment = null)
        {
            if (newStatus is not EventRequestStatus.Approved and not EventRequestStatus.Rejected)
            {
                throw new InvalidOperationException("Event request can only be approved or rejected.");
            }

            EventRequestDto reviewedDto;
            EventRequestReviewNotificationDto reviewNotification;

            await using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                var request = await _dbContext.EventRequests
                    .Include(r => r.Faculty)
                    .Include(r => r.Manager)
                    .Include(r => r.TargetAcademicEvent)
                        .ThenInclude(e => e!.Subscribers)
                    .FirstOrDefaultAsync(r => r.Id == requestId);

                if (request is null)
                {
                    throw new KeyNotFoundException("Event request was not found.");
                }

                if (request.Status != EventRequestStatus.Pending)
                {
                    throw new InvalidOperationException("Only pending event requests can be reviewed.");
                }

                var reviewedAtUtc = DateTime.UtcNow;
                ApprovedAcademicEventChange? approvedChange = null;

                request.Status = newStatus;
                request.AdminId = adminId;
                request.ReviewedAtUtc = reviewedAtUtc;
                request.ReviewComment = NormalizeOptional(reviewComment);

                if (newStatus == EventRequestStatus.Approved)
                {
                    approvedChange = await ApplyApprovedRequestAsync(request);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                reviewedDto = ToEventRequestDto(request);
                reviewNotification = BuildReviewNotification(request, approvedChange, reviewedAtUtc);
            }

            try
            {
                await _realtimeNotificationService.NotifyEventRequestReviewedAsync(reviewNotification);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to publish SignalR review notification for event request {EventRequestId}.",
                    reviewNotification.EventRequestId);
            }

            return reviewedDto;
        }

        private async Task<ApprovedAcademicEventChange?> ApplyApprovedRequestAsync(EventRequest request)
        {
            switch (request.RequestType)
            {
                case EventRequestType.Create:
                    var createdEvent = new AcademicEvent
                    {
                        Title = request.Title,
                        StartTime = request.StartTime,
                        EndTime = request.EndTime,
                        Location = request.Location,
                        FacultyId = request.FacultyId
                    };

                    request.TargetAcademicEvent = createdEvent;

                    return new ApprovedAcademicEventChange
                    {
                        Action = "created",
                        AcademicEvent = createdEvent,
                        UserRecipientIds = await GetFacultyUserIdsAsync(request.FacultyId)
                    };

                case EventRequestType.Update:
                    if (request.TargetAcademicEvent is null)
                    {
                        throw new InvalidOperationException("Target academic event was not found for update request.");
                    }

                    var updateRecipientIds = request.TargetAcademicEvent.Subscribers
                        .Select(subscriber => subscriber.Id)
                        .ToList();

                    request.TargetAcademicEvent.Title = request.Title;
                    request.TargetAcademicEvent.StartTime = request.StartTime;
                    request.TargetAcademicEvent.EndTime = request.EndTime;
                    request.TargetAcademicEvent.Location = request.Location;
                    request.TargetAcademicEvent.FacultyId = request.FacultyId;

                    return new ApprovedAcademicEventChange
                    {
                        Action = "updated",
                        AcademicEvent = request.TargetAcademicEvent,
                        UserRecipientIds = updateRecipientIds
                    };

                case EventRequestType.Delete:
                    if (request.TargetAcademicEvent is null)
                    {
                        throw new InvalidOperationException("Target academic event was not found for delete request.");
                    }

                    var target = request.TargetAcademicEvent;
                    var targetEventId = target.Id;
                    var deleteRecipientIds = target.Subscribers
                        .Select(subscriber => subscriber.Id)
                        .ToList();

                    var approvedChange = new ApprovedAcademicEventChange
                    {
                        Action = "deleted",
                        AcademicEvent = target,
                        UserRecipientIds = deleteRecipientIds
                    };

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
                    return approvedChange;

                default:
                    throw new InvalidOperationException("Unsupported event request type.");
            }
        }

        private async Task<IReadOnlyList<string>> GetFacultyUserIdsAsync(int facultyId)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .Where(user => user.Faculties.Any(faculty => faculty.Id == facultyId))
                .Select(user => user.Id)
                .ToListAsync();
        }

        private static EventRequestReviewNotificationDto BuildReviewNotification(
            EventRequest request,
            ApprovedAcademicEventChange? approvedChange,
            DateTime reviewedAtUtc)
        {
            var academicEvent = approvedChange?.AcademicEvent ?? request.TargetAcademicEvent;

            return new EventRequestReviewNotificationDto
            {
                EventRequestId = request.Id,
                RequestType = request.RequestType.ToString(),
                Status = request.Status.ToString(),
                AcademicEventId = academicEvent?.Id ?? request.TargetAcademicEventId,
                EventTitle = approvedChange?.AcademicEvent.Title ?? request.Title,
                StartTime = approvedChange?.AcademicEvent.StartTime ?? request.StartTime,
                EndTime = approvedChange?.AcademicEvent.EndTime ?? request.EndTime,
                Location = approvedChange?.AcademicEvent.Location ?? request.Location,
                FacultyId = request.FacultyId,
                FacultyName = request.Faculty.Name,
                FacultyDisplayName = request.Faculty.DisplayName,
                ManagerId = request.ManagerId,
                ManagerEmail = request.Manager.Email,
                ReviewComment = request.ReviewComment,
                ReviewedAtUtc = reviewedAtUtc,
                UserRecipientIds = approvedChange?.UserRecipientIds ?? Array.Empty<string>()
            };
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

        private sealed class ApprovedAcademicEventChange
        {
            public string Action { get; set; } = null!;
            public AcademicEvent AcademicEvent { get; set; } = null!;
            public IReadOnlyList<string> UserRecipientIds { get; set; } = Array.Empty<string>();
        }
    }
}
