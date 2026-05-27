using StudentPlanner.Api.Dtos.Schedule;

namespace StudentPlanner.Api.Services.Interfaces
{
    public interface IScheduleService
    {
        Task<IReadOnlyList<EventDto>> GetScheduleAsync(
            string userId,
            IReadOnlyCollection<string> roles,
            DateTime? from = null,
            DateTime? to = null);
    }
}