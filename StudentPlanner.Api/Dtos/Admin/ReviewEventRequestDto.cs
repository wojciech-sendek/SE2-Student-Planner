using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Api.Dtos.Admin
{
    public class ReviewEventRequestDto
    {
        public string? Status { get; set; }

        [MaxLength(1000)]
        public string? ReviewComment { get; set; }
    }
}
