using Microsoft.AspNetCore.Mvc;

namespace StudentPlanner.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Simple endpoint to verify that the API is running.
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Status = "OK",
                TimestampUtc = DateTime.UtcNow
            });
        }
    }
}