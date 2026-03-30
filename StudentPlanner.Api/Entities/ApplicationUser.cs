using Microsoft.AspNetCore.Identity;

namespace StudentPlanner.Api.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public ICollection<Faculty> Faculties { get; set; } = new List<Faculty>();
    }
}