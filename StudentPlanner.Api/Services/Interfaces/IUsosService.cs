using StudentPlanner.Api.Dtos.Usos;
using StudentPlanner.Api.Entities;

namespace StudentPlanner.Api.Services.Interfaces
{
    public interface IUsosService
    {
        UsosAuthorizationUrlResponseDto CreateAuthorizationUrl(string userId);
        Task CompleteAuthorizationAsync(string code, string state);
        Task<IReadOnlyList<UsosEventDto>> GetScheduleAsync(string userId, DateTime? from = null, DateTime? to = null);
        Task<IReadOnlyList<UsosEventDto>> SyncScheduleForUserAsync(ApplicationUser user);
    }
}