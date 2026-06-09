namespace StudentPlanner.Api.Dtos.Notifications
{
    public class AcademicEventChangeDto
    {
        public string Action { get; set; } = null!;
        public int? AcademicEventId { get; set; }
        public int EventRequestId { get; set; }
        public string RequestType { get; set; } = null!;
        public string EventTitle { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Location { get; set; }
        public int FacultyId { get; set; }
        public string FacultyName { get; set; } = null!;
        public string FacultyDisplayName { get; set; } = null!;
        public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
