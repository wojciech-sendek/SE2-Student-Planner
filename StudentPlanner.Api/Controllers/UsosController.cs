using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class UsosController : ControllerBase
    {
        private readonly IUsosMockService _usosService;

        public UsosController(IUsosMockService usosService)
        {
            _usosService = usosService;
        }

        [HttpPost("connect")]
        public async Task<IActionResult> Connect()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            await _usosService.ConnectAsync(userId);
            return Ok(new { Message = "Mock USOS connected and schedule synced." });
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            return Ok(await _usosService.GetStatusAsync(userId));
        }

        [HttpPost("sync")]
        public async Task<IActionResult> Sync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            await _usosService.SyncAsync(userId);
            return Ok(new { Message = "Mock USOS schedule synced." });
        }

        [HttpDelete("disconnect")]
        public async Task<IActionResult> Disconnect()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            await _usosService.DisconnectAsync(userId);
            return NoContent();
        }
    }
}
