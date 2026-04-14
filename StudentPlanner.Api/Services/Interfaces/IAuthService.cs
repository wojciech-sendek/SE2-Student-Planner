using StudentPlanner.Api.Dtos.Auth;

namespace StudentPlanner.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Succeeded, IEnumerable<string> Errors)> RegisterAsync(RegisterRequestDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto);
        Task<CurrentUserDto?> GetCurrentUserAsync(string userId);
        Task<bool> DeleteAccountAsync(string userId);
    }
}