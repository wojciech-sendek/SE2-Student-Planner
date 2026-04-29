using StudentPlanner.Api.Dtos.Auth;

namespace StudentPlanner.Api.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Succeeded, IEnumerable<string> Errors, RegisterResponseDto? Response)> RegisterAsync(RegisterRequestDto dto);
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto);
        Task<CurrentUserDto?> GetCurrentUserAsync(string userId);
        Task<bool> DeleteAccountAsync(string userId);
        Task ForgotPasswordAsync(ForgotPasswordRequestDto dto);
        Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(ResetPasswordRequestDto dto);
    }
}