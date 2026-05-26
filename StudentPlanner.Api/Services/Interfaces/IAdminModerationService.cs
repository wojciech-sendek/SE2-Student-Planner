using StudentPlanner.Api.Dtos.EventRequests;
using StudentPlanner.Api.Entities;

namespace StudentPlanner.Api.Services.Interfaces
{
    public interface IAdminModerationService
    {
        Task<IReadOnlyList<EventRequestDto>> GetEventRequestsAsync(EventRequestStatus? status = null, int? facultyId = null);
        Task<EventRequestDto?> GetEventRequestAsync(int requestId);
        Task<EventRequestDto> ReviewEventRequestAsync(string adminId, int requestId, EventRequestStatus newStatus, string? reviewComment = null);
    }
}
