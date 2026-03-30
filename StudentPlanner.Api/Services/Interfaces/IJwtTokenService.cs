using StudentPlanner.Api.Dtos.Auth;
using StudentPlanner.Api.Entities;

namespace StudentPlanner.Api.Services.Interfaces
{
    public interface IJwtTokenService
    {
        Task<AuthResponseDto> CreateTokenAsync(ApplicationUser user);
    }
}