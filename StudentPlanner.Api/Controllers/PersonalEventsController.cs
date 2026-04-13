using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Api.Dtos.PersonalEvents;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class PersonalEventsController : ControllerBase
    {
        private readonly IPersonalEventService _personalEventService;

        public PersonalEventsController(IPersonalEventService personalEventService)
        {
            _personalEventService = personalEventService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersonalEventDto>>> GetMine(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var events = await _personalEventService.GetMineAsync(userId, from, to);
            return Ok(events);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PersonalEventDto>> GetById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var personalEvent = await _personalEventService.GetByIdAsync(id, userId);

            if (personalEvent is null)
            {
                return NotFound(new
                {
                    Message = "Personal event not found."
                });
            }

            return Ok(personalEvent);
        }

        [HttpPost]
        public async Task<ActionResult<PersonalEventDto>> Create([FromBody] CreatePersonalEventRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var created = await _personalEventService.CreateAsync(userId, dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                created);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<PersonalEventDto>> Update(int id, [FromBody] UpdatePersonalEventRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var updated = await _personalEventService.UpdateAsync(id, userId, dto);

            if (updated is null)
            {
                return NotFound(new
                {
                    Message = "Personal event not found."
                });
            }

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var deleted = await _personalEventService.DeleteAsync(id, userId);

            if (!deleted)
            {
                return NotFound(new
                {
                    Message = "Personal event not found."
                });
            }

            return NoContent();
        }
    }
}