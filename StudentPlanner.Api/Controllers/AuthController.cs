using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Api.Dtos.Auth;
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

        /// <summary>
        /// Registers a new user account using a university email address.
        /// </summary>
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
                return BadRequest(new
                {
                    Message = "Registration failed.",
                    Errors = result.Errors
                });
            }

            return Ok(new
            {
                Message = "Registration successful."
            });
        }

        /// <summary>
        /// Logs in an existing user and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var authResponse = await _authService.LoginAsync(dto);

            if (authResponse is null)
            {
                return Unauthorized(new
                {
                    Message = "Invalid credentials."
                });
            }

            return Ok(authResponse);
        }

        /// <summary>
        /// Returns information about the currently authenticated user.
        /// </summary>
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

        /// <summary>
        /// Deletes the currently authenticated user's account.
        /// </summary>
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