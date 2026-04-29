using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Api.Dtos.Auth
{
    public class RegisterRequestDto : IValidatableObject
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string ConfirmPassword { get; set; } = null!;

        public int? FacultyId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = null!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.Equals(Password, ConfirmPassword, StringComparison.Ordinal))
            {
                yield return new ValidationResult(
                    "Passwords don't match.",
                    new[] { nameof(Password), nameof(ConfirmPassword) });
            }
        }
    }
}