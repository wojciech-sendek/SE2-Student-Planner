using System.ComponentModel.DataAnnotations;

namespace StudentPlanner.Api.Dtos.EventRequests
{
    public class DeleteEventRequestDto
    {
        [StringLength(1000)]
        public string? Reason { get; set; }
    }
}
