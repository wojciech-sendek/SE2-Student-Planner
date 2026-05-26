using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentPlanner.Api.Dtos.Admin;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUsersController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminUserDto>>> GetUsers()
        {
            var users = await _adminUserService.GetUsersAsync();
            return Ok(users);
        }

        [HttpPost("managers")]
        public async Task<ActionResult<AdminUserDto>> CreateManager([FromBody] CreateManagerDto dto)
        {
            try
            {
                var created = await _adminUserService.CreateManagerAsync(dto);
                return Created($"/api/admin/users/{created.Id}", created);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(currentAdminId))
            {
                return Unauthorized();
            }

            try
            {
                var deleted = await _adminUserService.DeleteUserAsync(currentAdminId, id);
                return deleted ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
