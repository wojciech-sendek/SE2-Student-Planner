namespace StudentPlanner.Api.Dtos.AcademicEvents
{
    public class AcademicEventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Location { get; set; }
        public int FacultyId { get; set; }
        public string FacultyName { get; set; } = null!;
        public string FacultyDisplayName { get; set; } = null!;
        public string Source { get; set; } = "faculty";
        public bool IsReadOnly { get; set; } = true;
    }
}
