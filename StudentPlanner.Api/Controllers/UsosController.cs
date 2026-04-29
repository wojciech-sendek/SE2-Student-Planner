using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Api.Services;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsosController : ControllerBase
    {
        private readonly IUsosService _usosService;

        public UsosController(IUsosService usosService)
        {
            _usosService = usosService;
        }

        [HttpGet("authorization-url")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetAuthorizationUrl()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var url = await _usosService.BuildAuthorizationUrlAsync(userId);
            return Ok(new { Url = url });
        }

        [HttpGet("callback")]
        [AllowAnonymous]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            {
                return BadRequest(new { Message = "Missing USOS OAuth code or state." });
            }

            try
            {
                await _usosService.CompleteAuthorizationAsync(code, state);
                return Ok(new { Message = "USOS authorization completed and schedule synced." });
            }
            catch (UsosApiException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("status")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Status()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            return Ok(await _usosService.GetStatusAsync(userId));
        }

        [HttpPost("sync")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Sync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            try
            {
                await _usosService.SyncAsync(userId);
                return Ok(new { Message = "USOS schedule synced." });
            }
            catch (UsosAuthorizationRequiredException)
            {
                return Conflict(new { Message = "USOS authorization required." });
            }
            catch (UsosApiException)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Message = "USOS API failure." });
            }
        }

        [HttpDelete("disconnect")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Disconnect()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            await _usosService.DisconnectAsync(userId);
            return NoContent();
        }
    }
}