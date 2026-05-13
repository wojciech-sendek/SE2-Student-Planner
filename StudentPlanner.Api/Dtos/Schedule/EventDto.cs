namespace StudentPlanner.Api.Dtos.Schedule
{
    public class EventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Location { get; set; }
        public string EventType { get; set; } = null!;
        public bool IsPersonal { get; set; }
        public string? Room { get; set; }
        public string? Teacher { get; set; }
    }
}