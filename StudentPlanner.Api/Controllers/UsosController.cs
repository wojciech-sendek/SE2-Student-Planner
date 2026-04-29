using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Api.Dtos.Usos;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Controllers
{
    [ApiController]
    [Route("api/usos")]
    public class UsosController : ControllerBase
    {
        private readonly IUsosService _usosService;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsosController(
            IUsosService usosService,
            UserManager<ApplicationUser> userManager)
        {
            _usosService = usosService;
            _userManager = userManager;
        }

        [HttpGet("authorization-url")]
        [Authorize]
        public IActionResult GetAuthorizationUrl()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            return Ok(_usosService.CreateAuthorizationUrl(userId));
        }

        [HttpGet("oauth-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> OAuthCallback(
            [FromQuery] string code,
            [FromQuery] string state)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            {
                return BadRequest(new
                {
                    Message = "Missing USOS OAuth code or state."
                });
            }

            try
            {
                await _usosService.CompleteAuthorizationAsync(code, state);

                return Ok(new
                {
                    Message = "USOS authorization finished. You can close this window and log in."
                });
            }
            catch (UsosApiException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    Message = "USOS API failure."
                });
            }
        }

        [HttpPost("exchange-code")]
        [AllowAnonymous]
        public async Task<IActionResult> ExchangeCode([FromBody] UsosExchangeCodeRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                await _usosService.CompleteAuthorizationAsync(dto.Code, dto.State);

                return Ok(new
                {
                    Message = "USOS authorization finished."
                });
            }
            catch (UsosApiException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new
                {
                    Message = "USOS API failure."
                });
            }
        }

        [HttpPost("sync")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IEnumerable<UsosEventDto>>> Sync()
        {
            var user = await GetCurrentApplicationUserAsync();

            if (user is null)
            {
                return Unauthorized();
            }

            try
            {
                var events = await _usosService.SyncScheduleForUserAsync(user);
                return Ok(events);
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
        }

        [HttpGet("status")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<UsosStatusDto>> GetStatus()
        {
            var user = await GetCurrentApplicationUserAsync();

            if (user is null)
            {
                return Unauthorized();
            }

            var events = await _usosService.GetScheduleAsync(user.Id);

            return Ok(new UsosStatusDto
            {
                IsConnected = !string.IsNullOrWhiteSpace(user.UsosRefreshTokenProtected),
                SyncedEventsCount = events.Count
            });
        }

        [HttpGet("events")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IEnumerable<UsosEventDto>>> GetEvents(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var events = await _usosService.GetScheduleAsync(userId, from, to);
            return Ok(events);
        }

        private async Task<ApplicationUser?> GetCurrentApplicationUserAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return string.IsNullOrWhiteSpace(userId)
                ? null
                : await _userManager.FindByIdAsync(userId);
        }
    }
}
