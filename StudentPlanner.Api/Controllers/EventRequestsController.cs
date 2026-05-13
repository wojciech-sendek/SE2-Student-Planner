using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Api.Dtos.EventRequests;
using StudentPlanner.Api.Services;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Controllers
{
    [ApiController]
    [Route("api/event-requests")]
    [Authorize(Roles = "Manager")]
    public class EventRequestsController : ControllerBase
    {
        private readonly IManagerEventRequestService _managerEventRequestService;

        public EventRequestsController(IManagerEventRequestService managerEventRequestService)
        {
            _managerEventRequestService = managerEventRequestService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventRequestDto>>> GetMine()
        {
            var managerId = GetCurrentUserId();

            if (managerId is null)
            {
                return Unauthorized();
            }

            var requests = await _managerEventRequestService.GetMineAsync(managerId);
            return Ok(requests);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventRequestDto>> GetMineById(int id)
        {
            var managerId = GetCurrentUserId();

            if (managerId is null)
            {
                return Unauthorized();
            }

            var request = await _managerEventRequestService.GetMineByIdAsync(managerId, id);

            if (request is null)
            {
                return NotFound(new { Message = "Event request not found." });
            }

            return Ok(request);
        }

        [HttpPost("create")]
        public async Task<ActionResult<EventRequestDto>> SubmitCreateRequest([FromBody] CreateEventRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var managerId = GetCurrentUserId();

            if (managerId is null)
            {
                return Unauthorized();
            }

            try
            {
                var created = await _managerEventRequestService.SubmitCreateRequestAsync(managerId, dto);
                return CreatedAtAction(nameof(GetMineById), new { id = created.Id }, created);
            }
            catch (ManagerFacultyNotAssignedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = ex.Message });
            }
            catch (ManagerFacultyAccessDeniedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = ex.Message });
            }
        }

        [HttpPost("update/{academicEventId:int}")]
        public async Task<ActionResult<EventRequestDto>> SubmitUpdateRequest(
            int academicEventId,
            [FromBody] UpdateEventRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var managerId = GetCurrentUserId();

            if (managerId is null)
            {
                return Unauthorized();
            }

            try
            {
                var created = await _managerEventRequestService.SubmitUpdateRequestAsync(managerId, academicEventId, dto);
                return CreatedAtAction(nameof(GetMineById), new { id = created.Id }, created);
            }
            catch (AcademicEventNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ManagerFacultyNotAssignedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = ex.Message });
            }
            catch (ManagerFacultyAccessDeniedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = ex.Message });
            }
        }

        [HttpPost("delete/{academicEventId:int}")]
        public async Task<ActionResult<EventRequestDto>> SubmitDeleteRequest(
            int academicEventId,
            [FromBody] DeleteEventRequestDto? dto = null)
        {
            var managerId = GetCurrentUserId();

            if (managerId is null)
            {
                return Unauthorized();
            }

            try
            {
                var created = await _managerEventRequestService.SubmitDeleteRequestAsync(managerId, academicEventId, dto);
                return CreatedAtAction(nameof(GetMineById), new { id = created.Id }, created);
            }
            catch (AcademicEventNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (ManagerFacultyNotAssignedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = ex.Message });
            }
            catch (ManagerFacultyAccessDeniedException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { Message = ex.Message });
            }
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
