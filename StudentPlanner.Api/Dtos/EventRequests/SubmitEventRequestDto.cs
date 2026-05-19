using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StudentPlanner.Api.Dtos.EventRequests
{
    public class SubmitEventRequestDto
    {
        [Required]
        public int RequestType { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? FacultyId { get; set; }

        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? TargetEventId { get; set; }

        public EventRequestDetailsDto? Details { get; set; }

        [StringLength(1000)]
        public string? Reason { get; set; }
    }

    public class EventRequestDetailsDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [StringLength(300)]
        public string? Location { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }
    }
}
