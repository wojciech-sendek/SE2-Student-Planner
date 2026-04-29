using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Api.Dtos.Auth;
using StudentPlanner.Api.Services;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.RegisterAsync(dto);

            if (!result.Succeeded)
            {
                var errors = result.Errors.ToList();

                if (errors.Any(e => e.Contains("already exists", StringComparison.OrdinalIgnoreCase)))
                {
                    return Conflict(new
                    {
                        Message = "Registration failed.",
                        Errors = errors
                    });
                }

                return BadRequest(new
                {
                    Message = "Registration failed.",
                    Errors = errors
                });
            }

            return Ok(result.Response);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            AuthResponseDto? authResponse;

            try
            {
                authResponse = await _authService.LoginAsync(dto);
            }
            catch (UsosAuthorizationRequiredException)
            {
                return Conflict(new
                {
                    Message = "USOS authorization required."
                });
            }
            catch (UsosApiException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    Message = "USOS API failure."
                });
            }

            if (authResponse is null)
            {
                return Unauthorized(new
                {
                    Message = "Invalid Credentials"
                });
            }

            return Ok(authResponse);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            await _authService.ForgotPasswordAsync(dto);

            return Ok(new
            {
                Message = "If the email exists, a reset password token was sent."
            });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.ResetPasswordAsync(dto);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    Message = "Password reset failed.",
                    Errors = result.Errors
                });
            }

            return Ok(new
            {
                Message = "Password Updated"
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var currentUser = await _authService.GetCurrentUserAsync(userId);

            if (currentUser is null)
            {
                return NotFound();
            }

            return Ok(currentUser);
        }

        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var succeeded = await _authService.DeleteAccountAsync(userId);

            if (!succeeded)
            {
                return BadRequest(new
                {
                    Message = "Failed to delete account."
                });
            }

            return NoContent();
        }
    }
}