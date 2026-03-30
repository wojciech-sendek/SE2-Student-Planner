using Microsoft.AspNetCore.Identity;

namespace StudentPlanner.Api.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public int? FacultyId { get; set; }
        public Faculty? Faculty { get; set; }
    }
}