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
        private readonly IUsosService _usosService;
        private readonly IEmailSender _emailSender;

        //TODO: move to config
        private static readonly string[] AllowedEmailDomains =
        {
            "pw.edu.pl"
        };

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            ApplicationDbContext dbContext,
            IUsosService usosService,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _dbContext = dbContext;
            _usosService = usosService;
            _emailSender = emailSender;
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> RegisterAsync(RegisterRequestDto dto)
        {
            var errors = new List<string>();

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
                .FirstOrDefaultAsync(f => f.Name == "university");

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

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains("User"))
            {
                await _usosService.EnsureConnectedAndSyncedAsync(user.Id);
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

        public async Task<bool> DeleteAccountAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return false;
            }

            var result = await _userManager.DeleteAsync(user);

            return result.Succeeded;
        }
        public async Task RequestPasswordResetAsync(ForgotPasswordRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            // Celowo nie ujawniamy, czy email istnieje.
            if (user is null || string.IsNullOrWhiteSpace(user.Email))
            {
                return;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailSender.SendPasswordResetTokenAsync(user.Email, token);
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            var errors = new List<string>();

            if (dto.NewPassword != dto.ConfirmPassword)
            {
                errors.Add("Passwords don't match.");
                return (false, errors);
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
            {
                errors.Add("Invalid token.");
                return (false, errors);
            }

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            if (!result.Succeeded)
            {
                errors.AddRange(result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            return (true, Enumerable.Empty<string>());
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