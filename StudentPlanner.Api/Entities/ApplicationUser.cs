using Microsoft.AspNetCore.Identity;

namespace StudentPlanner.Api.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Encrypted with ASP.NET Core Data Protection. Never store USOS passwords.
        public string? UsosRefreshTokenProtected { get; set; }
        public DateTime? UsosConnectedAtUtc { get; set; }
        public DateTime? UsosScheduleSyncedAtUtc { get; set; }

        public ICollection<Faculty> Faculties { get; set; } = new List<Faculty>();
        public ICollection<PersonalEvent> PersonalEvents { get; set; } = new List<PersonalEvent>();
        public ICollection<UsosEvent> UsosEvents { get; set; } = new List<UsosEvent>();
    }
}