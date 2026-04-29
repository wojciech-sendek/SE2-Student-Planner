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
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class AcademicEventsController : ControllerBase
    {
        private readonly IUsosService _usosService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AcademicEventsController(
            IUsosService usosService,
            UserManager<ApplicationUser> userManager)
        {
            _usosService = usosService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsosEventDto>>> GetMine(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return Unauthorized();
            }

            try
            {
                await _usosService.SyncScheduleForUserAsync(user);

                var events = await _usosService.GetScheduleAsync(userId, from, to);

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
    }
}