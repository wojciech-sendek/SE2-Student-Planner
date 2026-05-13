using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Api.Dtos.EventRequests
{
    public class CreateEventRequestDto : IValidatableObject
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [StringLength(300)]
        public string? Location { get; set; }

        // Optional. If frontend sends it, backend verifies that it equals the manager's assigned faculty.
        public int? FacultyId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndTime <= StartTime)
            {
                yield return new ValidationResult(
                    "EndTime must be greater than StartTime.",
                    new[] { nameof(EndTime), nameof(StartTime) });
            }
        }
    }
}
