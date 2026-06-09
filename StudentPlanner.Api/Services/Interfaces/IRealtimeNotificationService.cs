using StudentPlanner.Api.Dtos.Notifications;

namespace StudentPlanner.Api.Services.Interfaces
{
    public interface IRealtimeNotificationService
    {
        Task NotifyEventRequestReviewedAsync(EventRequestReviewNotificationDto reviewNotification);
    }
}
