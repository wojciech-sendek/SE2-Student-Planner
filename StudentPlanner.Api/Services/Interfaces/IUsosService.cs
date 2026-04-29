using StudentPlanner.Api.Dtos.Usos;

namespace StudentPlanner.Api.Services.Interfaces
{
    public interface IUsosService
    {
        Task<string> BuildAuthorizationUrlAsync(string userId);
        Task CompleteAuthorizationAsync(string code, string state);

        Task EnsureConnectedAndSyncedAsync(string userId);
        Task SyncAsync(string userId);
        Task DisconnectAsync(string userId);

        Task<UsosStatusDto> GetStatusAsync(string userId);
        Task<IReadOnlyList<AcademicEventDto>> GetAcademicEventsAsync(
            string userId,
            DateTime? from = null,
            DateTime? to = null);
    }
}