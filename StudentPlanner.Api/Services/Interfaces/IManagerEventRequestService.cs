using StudentPlanner.Api.Dtos.AcademicEvents;
using StudentPlanner.Api.Dtos.EventRequests;

namespace StudentPlanner.Api.Services.Interfaces
{
    public interface IManagerEventRequestService
    {
        Task<IReadOnlyList<AcademicEventDto>> GetVisibleAcademicEventsAsync(string managerId, DateTime? from = null, DateTime? to = null);
        Task<IReadOnlyList<EventRequestDto>> GetMineAsync(string managerId);
        Task<EventRequestDto?> GetMineByIdAsync(string managerId, int requestId);
        Task<EventRequestDto> SubmitCreateRequestAsync(string managerId, CreateEventRequestDto dto);
        Task<EventRequestDto> SubmitUpdateRequestAsync(string managerId, int academicEventId, UpdateEventRequestDto dto);
        Task<EventRequestDto> SubmitDeleteRequestAsync(string managerId, int academicEventId, DeleteEventRequestDto? dto = null);
    }
}
