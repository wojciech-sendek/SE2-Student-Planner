namespace StudentPlanner.Api.Entities
{
    public class EventRequest
    {
        public int Id { get; set; }

        public EventRequestType RequestType { get; set; }
        public EventRequestStatus Status { get; set; } = EventRequestStatus.Pending;

        public int? TargetAcademicEventId { get; set; }
        public AcademicEvent? TargetAcademicEvent { get; set; }

        public string Title { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Location { get; set; }

        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; } = null!;

        public string ManagerId { get; set; } = null!;
        public ApplicationUser Manager { get; set; } = null!;

        public DateTime CreatedAtUtc { get; set; }
        public DateTime? ReviewedAtUtc { get; set; }

        public string? AdminId { get; set; }
        public ApplicationUser? Admin { get; set; }
        public string? ReviewComment { get; set; }
    }
}
