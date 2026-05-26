using StudentPlanner.Api.Dtos.AcademicEvents;

namespace StudentPlanner.Api.Services.Interfaces
{
    public interface IAcademicEventSubscriptionService
    {
        Task<IReadOnlyList<AcademicEventDto>> GetAvailableAsync(string userId, int? facultyId = null, DateTime? from = null, DateTime? to = null);
        Task<IReadOnlyList<AcademicEventDto>> GetSubscribedAsync(string userId, DateTime? from = null, DateTime? to = null);
        Task<AcademicEventDto?> SubscribeAsync(string userId, int academicEventId);
        Task<AcademicEventDto?> UnsubscribeAsync(string userId, int academicEventId);
    }
}
