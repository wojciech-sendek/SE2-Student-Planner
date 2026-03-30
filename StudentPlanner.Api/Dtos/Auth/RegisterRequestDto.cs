using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Api.Dtos.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;

        public int? FacultyId { get; set; }

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;
    }
}