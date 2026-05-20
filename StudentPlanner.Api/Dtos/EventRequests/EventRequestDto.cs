namespace StudentPlanner.Api.Dtos.EventRequests
{
    public class EventRequestDto
    {
        public int Id { get; set; }
        public int RequestId => Id;
        public string RequestType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string RequestStatus => Status;
        public int? TargetAcademicEventId { get; set; }
        public string Title { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Location { get; set; }
        public EventRequestDetailsDto Details => new()
        {
            Title = Title,
            StartTime = StartTime,
            EndTime = EndTime,
            Location = Location
        };
        public int FacultyId { get; set; }
        public string FacultyName { get; set; } = null!;
        public string FacultyDisplayName { get; set; } = null!;
        public string ManagerId { get; set; } = null!;
        public string? ManagerEmail { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime SubmissionDate => CreatedAtUtc;
        public DateTime? ReviewedAtUtc { get; set; }
        public string? ReviewComment { get; set; }
    }
}
