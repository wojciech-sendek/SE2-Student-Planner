using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.Auth;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ApplicationDbContext _dbContext;

        //TODO: move to config
        private static readonly string[] AllowedEmailDomains =
        {
            "pw.edu.pl"
        };

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _dbContext = dbContext;
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> RegisterAsync(RegisterRequestDto dto)
        {
            var errors = new List<string>();

            if (dto.Password != dto.ConfirmPassword)
            {
                errors.Add("Passwords do not match.");
                return (false, errors);
            }

            if (!IsAllowedUniversityEmail(dto.Email))
            {
                errors.Add("Please enter a valid university email.");
                return (false, errors);
            }

            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (existingUser is not null)
            {
                errors.Add("An account with this email already exists.");
                return (false, errors);
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            var universityFaculty = await _dbContext.Faculties
                .FirstOrDefaultAsync(f => f.Name == "University");

            if (universityFaculty is not null)
                user.Faculties.Add(universityFaculty);

            if (dto.FacultyId.HasValue)
            {
                var departmentFaculty = await _dbContext.Faculties
                    .FirstOrDefaultAsync(f => f.Id == dto.FacultyId.Value);

                if (departmentFaculty is not null)
                    user.Faculties.Add(departmentFaculty);
            }

            await _userManager.UpdateAsync(user);
            await _userManager.AddToRoleAsync(user, "User");

            return (true, Enumerable.Empty<string>());
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user is null)
            {
                return null;
            }

            var validPassword = await _userManager.CheckPasswordAsync(user, dto.Password);

            if (!validPassword)
            {
                return null;
            }

            return await _jwtTokenService.CreateTokenAsync(user);
        }

        public async Task<CurrentUserDto?> GetCurrentUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new CurrentUserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                Roles = roles
            };
        }

        private static bool IsAllowedUniversityEmail(string email)
        {
            var atIndex = email.LastIndexOf('@');
            if (atIndex < 0 || atIndex == email.Length - 1)
            {
                return false;
            }

            var domain = email[(atIndex + 1)..].Trim().ToLowerInvariant();
            return AllowedEmailDomains.Contains(domain);
        }
    }
}