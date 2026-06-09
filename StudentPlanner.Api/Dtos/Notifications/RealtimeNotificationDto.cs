namespace StudentPlanner.Api.Dtos.Notifications
{
    public class RealtimeNotificationDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Type { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public string Severity { get; set; } = "info";
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public int? EventRequestId { get; set; }
        public int? AcademicEventId { get; set; }
        public int? FacultyId { get; set; }
        public string? FacultyName { get; set; }
        public string? FacultyDisplayName { get; set; }
        public string? RequestType { get; set; }
        public string? RequestStatus { get; set; }
    }
}
