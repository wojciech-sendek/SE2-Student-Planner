using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Api.Dtos.AcademicEvents;
using StudentPlanner.Api.Services;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Controllers
{
    [ApiController]
    [Route("api/manager/academic-events")]
    [Authorize(Roles = "Manager")]
    public class ManagerAcademicEventsController : ControllerBase
    {
        private readonly IManagerEventRequestService _managerEventRequestService;

        public ManagerAcademicEventsController(IManagerEventRequestService managerEventRequestService)
        {
            _managerEventRequestService = managerEventRequestService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AcademicEventDto>>> GetVisibleAcademicEvents(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var managerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(managerId))
            {
                return Unauthorized();
            }

            try
            {
                var events = await _managerEventRequestService.GetVisibleAcademicEventsAsync(managerId, from, to);
                return Ok(events);
            }
            catch (ManagerFacultyNotAssignedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = ex.Message });
            }
        }
    }
}
