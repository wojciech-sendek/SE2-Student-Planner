using Microsoft.AspNetCore.SignalR;
using StudentPlanner.Api.Dtos.Notifications;
using StudentPlanner.Api.Hubs;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class RealtimeNotificationService : IRealtimeNotificationService
    {
        private readonly IHubContext<NotificationsHub> _hubContext;
        private readonly ILogger<RealtimeNotificationService> _logger;

        public RealtimeNotificationService(
            IHubContext<NotificationsHub> hubContext,
            ILogger<RealtimeNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyEventRequestReviewedAsync(EventRequestReviewNotificationDto reviewNotification)
        {
            if (string.IsNullOrWhiteSpace(reviewNotification.ManagerId))
            {
                _logger.LogWarning(
                    "Skipping SignalR manager notification for reviewed request {EventRequestId}: manager id is missing.",
                    reviewNotification.EventRequestId);

                return;
            }

            var managerNotification = BuildManagerNotification(reviewNotification);

            await _hubContext.Clients.User(reviewNotification.ManagerId)
                .SendAsync("ReceiveNotification", managerNotification);

            await _hubContext.Clients.User(reviewNotification.ManagerId)
                .SendAsync("EventRequestReviewed", reviewNotification);

            if (!reviewNotification.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var userRecipientIds = reviewNotification.UserRecipientIds
                .Where(userId => !string.IsNullOrWhiteSpace(userId))
                .Where(userId => !string.Equals(userId, reviewNotification.ManagerId, StringComparison.Ordinal))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (userRecipientIds.Count == 0)
            {
                return;
            }

            var academicEventChange = BuildAcademicEventChange(reviewNotification);
            var userNotification = BuildUserAcademicEventNotification(reviewNotification, academicEventChange.Action);

            await _hubContext.Clients.Users(userRecipientIds)
                .SendAsync("ReceiveNotification", userNotification);

            await _hubContext.Clients.Users(userRecipientIds)
                .SendAsync("AcademicEventChanged", academicEventChange);
        }

        private static RealtimeNotificationDto BuildManagerNotification(EventRequestReviewNotificationDto reviewNotification)
        {
            var approved = reviewNotification.Status.Equals("Approved", StringComparison.OrdinalIgnoreCase);
            var statusText = approved ? "approved" : "rejected";

            return new RealtimeNotificationDto
            {
                Type = $"eventRequest.{statusText}",
                Title = approved ? "Event request approved" : "Event request rejected",
                Message = $"Your {reviewNotification.RequestType.ToLowerInvariant()} request for '{reviewNotification.EventTitle}' was {statusText}.",
                Severity = approved ? "success" : "warning",
                EventRequestId = reviewNotification.EventRequestId,
                AcademicEventId = reviewNotification.AcademicEventId,
                FacultyId = reviewNotification.FacultyId,
                FacultyName = reviewNotification.FacultyName,
                FacultyDisplayName = reviewNotification.FacultyDisplayName,
                RequestType = reviewNotification.RequestType,
                RequestStatus = reviewNotification.Status,
                CreatedAtUtc = reviewNotification.ReviewedAtUtc
            };
        }

        private static AcademicEventChangeDto BuildAcademicEventChange(EventRequestReviewNotificationDto reviewNotification)
        {
            return new AcademicEventChangeDto
            {
                Action = MapAction(reviewNotification.RequestType),
                AcademicEventId = reviewNotification.AcademicEventId,
                EventRequestId = reviewNotification.EventRequestId,
                RequestType = reviewNotification.RequestType,
                EventTitle = reviewNotification.EventTitle,
                StartTime = reviewNotification.StartTime,
                EndTime = reviewNotification.EndTime,
                Location = reviewNotification.Location,
                FacultyId = reviewNotification.FacultyId,
                FacultyName = reviewNotification.FacultyName,
                FacultyDisplayName = reviewNotification.FacultyDisplayName,
                ChangedAtUtc = reviewNotification.ReviewedAtUtc
            };
        }

        private static RealtimeNotificationDto BuildUserAcademicEventNotification(
            EventRequestReviewNotificationDto reviewNotification,
            string action)
        {
            var title = action switch
            {
                "created" => "New faculty event approved",
                "updated" => "Faculty event updated",
                "deleted" => "Faculty event cancelled",
                _ => "Faculty event changed"
            };

            var message = action switch
            {
                "created" => $"A new event '{reviewNotification.EventTitle}' has been approved for {reviewNotification.FacultyDisplayName}.",
                "updated" => $"The subscribed event '{reviewNotification.EventTitle}' has been updated.",
                "deleted" => $"The subscribed event '{reviewNotification.EventTitle}' has been cancelled.",
                _ => $"The event '{reviewNotification.EventTitle}' has changed."
            };

            var severity = action == "deleted" ? "warning" : "info";

            return new RealtimeNotificationDto
            {
                Type = $"academicEvent.{action}",
                Title = title,
                Message = message,
                Severity = severity,
                EventRequestId = reviewNotification.EventRequestId,
                AcademicEventId = reviewNotification.AcademicEventId,
                FacultyId = reviewNotification.FacultyId,
                FacultyName = reviewNotification.FacultyName,
                FacultyDisplayName = reviewNotification.FacultyDisplayName,
                RequestType = reviewNotification.RequestType,
                RequestStatus = reviewNotification.Status,
                CreatedAtUtc = reviewNotification.ReviewedAtUtc
            };
        }

        private static string MapAction(string requestType)
        {
            return requestType switch
            {
                "Create" => "created",
                "Update" => "updated",
                "Delete" => "deleted",
                _ => "changed"
            };
        }
    }
}
