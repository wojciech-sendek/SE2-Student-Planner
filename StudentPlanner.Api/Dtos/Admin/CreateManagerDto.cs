using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Api.Dtos.Admin
{
    public class CreateManagerDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;

        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        public int? FacultyId { get; set; }
    }
}
