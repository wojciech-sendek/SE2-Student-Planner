using StudentPlanner.Api.Dtos.Admin;

namespace StudentPlanner.Api.Services.Interfaces
{
    public interface IAdminUserService
    {
        Task<IReadOnlyList<AdminUserDto>> GetUsersAsync();
        Task<AdminUserDto> CreateManagerAsync(CreateManagerDto dto);
        Task<bool> DeleteUserAsync(string currentAdminId, string userId);
    }
}
