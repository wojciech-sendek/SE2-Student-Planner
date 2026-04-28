namespace StudentPlanner.Api.Dtos.Usos
{
    public class AcademicEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Location { get; set; }
        public string? CourseId { get; set; }
        public string? LecturerName { get; set; }
        public string? Room { get; set; }
        public string EventType { get; set; } = "Academic";
        public bool IsPersonal { get; set; } = false;
        public bool IsReadOnly { get; set; } = true;
    }
}
