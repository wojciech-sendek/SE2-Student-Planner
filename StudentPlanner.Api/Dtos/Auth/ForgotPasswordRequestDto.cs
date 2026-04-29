using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Api.Dtos.Auth
{
    public class ForgotPasswordRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
