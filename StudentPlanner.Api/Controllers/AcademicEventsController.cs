using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Api.Dtos.Usos;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class AcademicEventsController : ControllerBase
    {
        private readonly IUsosService _usosService;

        public AcademicEventsController(IUsosService usosService)
        {
            _usosService = usosService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AcademicEventDto>>> GetMine([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            await _usosService.EnsureConnectedAndSyncedAsync(userId);
            return Ok(await _usosService.GetAcademicEventsAsync(userId, from, to));
        }
    }
}
