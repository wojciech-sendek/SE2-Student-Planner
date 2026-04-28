using StudentPlanner.Api.Dtos.Usos;

namespace StudentPlanner.Api.Services.Interfaces
{
    public interface IUsosMockService
    {
        Task EnsureConnectedAndSyncedAsync(string userId);
        Task ConnectAsync(string userId);
        Task SyncAsync(string userId);
        Task DisconnectAsync(string userId);
        Task<UsosStatusDto> GetStatusAsync(string userId);
        Task<IReadOnlyList<AcademicEventDto>> GetAcademicEventsAsync(string userId, DateTime? from = null, DateTime? to = null);
    }
}
