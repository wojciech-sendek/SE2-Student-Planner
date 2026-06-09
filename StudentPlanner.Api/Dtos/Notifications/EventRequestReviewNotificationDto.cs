using System.Text.Json.Serialization;

namespace StudentPlanner.Api.Dtos.Notifications
{
    public class EventRequestReviewNotificationDto
    {
        public int EventRequestId { get; set; }
        public string RequestType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int? AcademicEventId { get; set; }
        public string EventTitle { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Location { get; set; }
        public int FacultyId { get; set; }
        public string FacultyName { get; set; } = null!;
        public string FacultyDisplayName { get; set; } = null!;
        public string ManagerId { get; set; } = null!;
        public string? ManagerEmail { get; set; }
        public string? ReviewComment { get; set; }
        public DateTime ReviewedAtUtc { get; set; }

        [JsonIgnore]
        public IReadOnlyList<string> UserRecipientIds { get; set; } = Array.Empty<string>();
    }
}
