using StudentPlanner.Api.Dtos.PersonalEvents;

namespace StudentPlanner.Api.Services.Interfaces
{
    public interface IPersonalEventService
    {
        Task<IReadOnlyList<PersonalEventDto>> GetMineAsync(string userId, DateTime? from = null, DateTime? to = null);
        Task<PersonalEventDto?> GetByIdAsync(int id, string userId);
        Task<PersonalEventDto> CreateAsync(string userId, CreatePersonalEventRequestDto dto);
        Task<PersonalEventDto?> UpdateAsync(int id, string userId, UpdatePersonalEventRequestDto dto);
        Task<bool> DeleteAsync(int id, string userId);
    }
}