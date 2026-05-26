using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.Admin;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class AdminUserService : IAdminUserService
    {
        private const string ManagerRole = "Manager";
        private const string AdminRole = "Admin";

        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminUserService(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IReadOnlyList<AdminUserDto>> GetUsersAsync()
        {
            var users = await _userManager.Users
                .Include(u => u.Faculties)
                .OrderBy(u => u.Email)
                .ToListAsync();

            var result = new List<AdminUserDto>(users.Count);
            foreach (var user in users)
            {
                result.Add(await ToDtoAsync(user));
            }

            return result;
        }

        public async Task<AdminUserDto> CreateManagerAsync(CreateManagerDto dto)
        {
            if (!await _roleManager.RoleExistsAsync(ManagerRole))
            {
                var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(ManagerRole));
                EnsureIdentitySucceeded(createRoleResult, "Manager role could not be created.");
            }

            if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var manager = new ApplicationUser
            {
                UserName = dto.Email.Trim(),
                Email = dto.Email.Trim(),
                EmailConfirmed = true,
                FirstName = NormalizeOptional(dto.FirstName),
                LastName = NormalizeOptional(dto.LastName)
            };

            var createResult = await _userManager.CreateAsync(manager, dto.Password);
            EnsureIdentitySucceeded(createResult, "Manager user could not be created.");

            var roleResult = await _userManager.AddToRoleAsync(manager, ManagerRole);
            EnsureIdentitySucceeded(roleResult, "Manager role could not be assigned.");

            manager = await _userManager.Users
                .Include(u => u.Faculties)
                .FirstAsync(u => u.Id == manager.Id);

            if (dto.FacultyId.HasValue)
            {
                var faculty = await _dbContext.Faculties.FindAsync(dto.FacultyId.Value)
                    ?? throw new KeyNotFoundException("Faculty was not found.");

                manager.Faculties.Add(faculty);
                await _dbContext.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return await ToDtoAsync(manager);
        }

        public async Task<bool> DeleteUserAsync(string currentAdminId, string userId)
        {
            if (currentAdminId == userId)
            {
                throw new InvalidOperationException("Admin cannot delete their own account.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return false;
            }

            if (await _userManager.IsInRoleAsync(user, AdminRole))
            {
                var admins = await _userManager.GetUsersInRoleAsync(AdminRole);
                if (admins.Count <= 1)
                {
                    throw new InvalidOperationException("Cannot delete the last admin account.");
                }
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            var reviewedRequests = await _dbContext.EventRequests
                .Where(request => request.AdminId == userId)
                .ToListAsync();

            foreach (var request in reviewedRequests)
            {
                request.AdminId = null;
            }

            var managedRequests = await _dbContext.EventRequests
                .Where(request => request.ManagerId == userId)
                .ToListAsync();

            _dbContext.EventRequests.RemoveRange(managedRequests);
            await _dbContext.SaveChangesAsync();

            var deleteResult = await _userManager.DeleteAsync(user);
            EnsureIdentitySucceeded(deleteResult, "User could not be deleted.");

            await transaction.CommitAsync();
            return true;
        }

        private async Task<AdminUserDto> ToDtoAsync(ApplicationUser user)
        {
            if (!_dbContext.Entry(user).Collection(u => u.Faculties).IsLoaded)
            {
                await _dbContext.Entry(user).Collection(u => u.Faculties).LoadAsync();
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.OrderBy(role => role).ToList(),
                FacultyIds = user.Faculties.Select(f => f.Id).OrderBy(id => id).ToList(),
                FacultyNames = user.Faculties.Select(f => f.DisplayName).OrderBy(name => name).ToList()
            };
        }

        private static void EnsureIdentitySucceeded(IdentityResult result, string message)
        {
            if (result.Succeeded)
            {
                return;
            }

            var errors = string.Join("; ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"{message} {errors}");
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
