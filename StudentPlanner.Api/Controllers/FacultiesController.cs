using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentPlanner.Api.Data;
using StudentPlanner.Api.Dtos.Faculty;

namespace StudentPlanner.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FacultiesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public FacultiesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Returns the list of faculties.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FacultyDto>>> GetAll()
        {
            var faculties = await _dbContext.Faculties
                .OrderBy(f => f.DisplayName)
                .Select(f => new FacultyDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    DisplayName = f.DisplayName
                })
                .ToListAsync();

            return Ok(faculties);
        }
    }
}