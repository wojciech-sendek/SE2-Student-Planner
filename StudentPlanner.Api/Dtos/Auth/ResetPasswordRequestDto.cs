using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Api.Dtos.Auth
{
    public class ResetPasswordRequestDto : IValidatableObject
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;

        // Opcjonalne: frontend może wysyłać, ale testy API tego nie wysyłają.
        [MinLength(8)]
        public string? ConfirmPassword { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(ConfirmPassword)
                && !string.Equals(NewPassword, ConfirmPassword, StringComparison.Ordinal))
            {
                yield return new ValidationResult(
                    "Passwords don't match.",
                    new[] { nameof(NewPassword), nameof(ConfirmPassword) });
            }
        }
    }
}