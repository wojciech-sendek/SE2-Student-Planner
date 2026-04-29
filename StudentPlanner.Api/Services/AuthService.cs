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
        private readonly IEmailService _emailService;
        private readonly IUsosService _usosService;

        private static readonly string[] AllowedEmailDomains =
        {
            "pw.edu.pl"
        };

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService,
            ApplicationDbContext dbContext,
            IEmailService emailService,
            IUsosService usosService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
            _dbContext = dbContext;
            _emailService = emailService;
            _usosService = usosService;
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors, RegisterResponseDto? Response)> RegisterAsync(RegisterRequestDto dto)
        {
            var errors = new List<string>();

            if (!string.IsNullOrWhiteSpace(dto.ConfirmPassword)
                && !string.Equals(dto.Password, dto.ConfirmPassword, StringComparison.Ordinal))
            {
                errors.Add("Passwords don't match.");
                return (false, errors, null);
            }

            if (!IsAllowedUniversityEmail(dto.Email))
            {
                errors.Add("Please enter a valid university email.");
                return (false, errors, null);
            }

            var email = dto.Email.Trim();

            var existingUser = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser is not null)
            {
                errors.Add("An account with this email already exists.");
                return (false, errors, null);
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password);

            if (!createResult.Succeeded)
            {
                errors.AddRange(createResult.Errors.Select(e => e.Description));
                return (false, errors, null);
            }

            var universityFaculty = await _dbContext.Faculties
                .FirstOrDefaultAsync(f => f.Name == "university");

            if (universityFaculty is not null)
            {
                user.Faculties.Add(universityFaculty);
            }

            if (dto.FacultyId.HasValue)
            {
                var departmentFaculty = await _dbContext.Faculties
                    .FirstOrDefaultAsync(f => f.Id == dto.FacultyId.Value);

                if (departmentFaculty is not null)
                {
                    user.Faculties.Add(departmentFaculty);
                }
            }

            await _userManager.UpdateAsync(user);

            var roleResult = await _userManager.AddToRoleAsync(user, "User");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                errors.AddRange(roleResult.Errors.Select(e => e.Description));
                return (false, errors, null);
            }

            var authorization = _usosService.CreateAuthorizationUrl(user.Id);

            return (true, Enumerable.Empty<string>(), new RegisterResponseDto
            {
                Message = "Registration successful.",
                UserId = user.Id,
                UsosAuthorizationUrl = authorization.AuthorizationUrl,
                UsosState = authorization.State
            });
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
            var isRegularUser = roles.Contains("User")
                                && !roles.Contains("Manager")
                                && !roles.Contains("Admin");

            if (isRegularUser)
            {
                await _usosService.SyncScheduleForUserAsync(user);
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

        public async Task ForgotPasswordAsync(ForgotPasswordRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
            {
                return;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var subject = "Student Planner - Password Reset";
            var body = $"""
                <p>Your password reset token is:</p>
                <p><b>{System.Net.WebUtility.HtmlEncode(token)}</b></p>
                <p>This token is time-limited. If you did not request a password reset, ignore this email.</p>
                """;

            await _emailService.SendEmailAsync(user.Email!, subject, body);
        }

        public async Task<(bool Succeeded, IEnumerable<string> Errors)> ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.ConfirmPassword)
                && !string.Equals(dto.NewPassword, dto.ConfirmPassword, StringComparison.Ordinal))
            {
                return (false, new[] { "Passwords don't match." });
            }

            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user is null)
            {
                return (false, new[] { "Invalid token." });
            }

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            if (!result.Succeeded)
            {
                return (false, new[] { "Invalid token." });
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