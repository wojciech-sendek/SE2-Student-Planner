using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Api.Dtos.AcademicEvents;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Controllers
{
    [ApiController]
    [Route("api/faculty-events")]
    [Authorize(Roles = "User")]
    public class FacultyAcademicEventsController : ControllerBase
    {
        private readonly IAcademicEventSubscriptionService _subscriptionService;

        public FacultyAcademicEventsController(IAcademicEventSubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AcademicEventDto>>> GetAvailable(
            [FromQuery] int? facultyId = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Unauthorized();
            }

            var events = await _subscriptionService.GetAvailableAsync(userId, facultyId, from, to);
            return Ok(events);
        }

        [HttpGet("subscribed")]
        public async Task<ActionResult<IEnumerable<AcademicEventDto>>> GetSubscribed(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Unauthorized();
            }

            var events = await _subscriptionService.GetSubscribedAsync(userId, from, to);
            return Ok(events);
        }

        [HttpPost("{id:int}/subscribe")]
        public async Task<ActionResult<AcademicEventDto>> Subscribe(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Unauthorized();
            }

            try
            {
                var academicEvent = await _subscriptionService.SubscribeAsync(userId, id);
                return academicEvent is null ? NotFound() : Ok(academicEvent);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}/subscribe")]
        public async Task<ActionResult<AcademicEventDto>> Unsubscribe(int id)
        {
            var userId = GetCurrentUserId();
            if (userId is null)
            {
                return Unauthorized();
            }

            var academicEvent = await _subscriptionService.UnsubscribeAsync(userId, id);
            return academicEvent is null ? NotFound() : Ok(academicEvent);
        }

        private string? GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrWhiteSpace(userId) ? null : userId;
        }
    }
}
