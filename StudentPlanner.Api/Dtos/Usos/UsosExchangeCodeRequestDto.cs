using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Api.Dtos.Usos
{
    public class UsosExchangeCodeRequestDto
    {
        [Required]
        public string Code { get; set; } = string.Empty;

        [Required]
        public string State { get; set; } = string.Empty;
    }
}