using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Api.Dtos.Admin;
using StudentPlanner.Api.Dtos.EventRequests;
using StudentPlanner.Api.Entities.Enums;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Controllers
{
    [ApiController]
    [Route("api/admin/event-requests")]
    [Authorize(Roles = "Admin")]
    public class AdminEventRequestsController : ControllerBase
    {
        private readonly IAdminModerationService _moderationService;

        public AdminEventRequestsController(IAdminModerationService moderationService)
        {
            _moderationService = moderationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventRequestDto>>> GetEventRequests(
            [FromQuery] string? status = null,
            [FromQuery] int? facultyId = null)
        {
            try
            {
                var parsedStatus = ParseOptionalStatus(status);
                var requests = await _moderationService.GetEventRequestsAsync(parsedStatus, facultyId);
                return Ok(requests);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventRequestDto>> GetEventRequest(int id)
        {
            var request = await _moderationService.GetEventRequestAsync(id);
            return request is null ? NotFound() : Ok(request);
        }

        [HttpPost("{id:int}/approve")]
        public async Task<ActionResult<EventRequestDto>> Approve(int id, [FromBody] ReviewEventRequestDto? dto = null)
        {
            return await Review(id, EventRequestStatus.Approved, dto?.ReviewComment);
        }

        [HttpPost("{id:int}/reject")]
        public async Task<ActionResult<EventRequestDto>> Reject(int id, [FromBody] ReviewEventRequestDto? dto = null)
        {
            return await Review(id, EventRequestStatus.Rejected, dto?.ReviewComment);
        }

        [HttpPatch("{id:int}/status")]
        public async Task<ActionResult<EventRequestDto>> ChangeStatus(int id, [FromBody] ReviewEventRequestDto dto)
        {
            try
            {
                var newStatus = ParseRequiredReviewStatus(dto.Status);
                return await Review(id, newStatus, dto.ReviewComment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task<ActionResult<EventRequestDto>> Review(int id, EventRequestStatus newStatus, string? reviewComment)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(adminId))
            {
                return Unauthorized();
            }

            try
            {
                var reviewed = await _moderationService.ReviewEventRequestAsync(adminId, id, newStatus, reviewComment);
                return Ok(reviewed);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static EventRequestStatus? ParseOptionalStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return null;
            }

            return ParseStatus(status);
        }

        private static EventRequestStatus ParseRequiredReviewStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                throw new ArgumentException("Status is required.");
            }

            var parsed = ParseStatus(status);
            if (parsed is not EventRequestStatus.Approved and not EventRequestStatus.Rejected)
            {
                throw new ArgumentException("Status must be Approved or Rejected.");
            }

            return parsed;
        }

        private static EventRequestStatus ParseStatus(string status)
        {
            if (Enum.TryParse<EventRequestStatus>(status, ignoreCase: true, out var parsed))
            {
                return parsed;
            }

            if (int.TryParse(status, out var numeric) && Enum.IsDefined(typeof(EventRequestStatus), numeric))
            {
                return (EventRequestStatus)numeric;
            }

            throw new ArgumentException("Invalid event request status.");
        }
    }
}
