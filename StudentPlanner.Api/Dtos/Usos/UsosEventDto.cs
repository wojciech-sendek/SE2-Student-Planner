namespace StudentPlanner.Api.Dtos.Usos
{
    public class UsosEventDto
    {
        public int Id { get; set; }
        public string ExternalId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Location { get; set; }
        public string? Room { get; set; }
        public string? Teacher { get; set; }
        public string Source { get; set; } = "usos";
        public bool IsReadOnly { get; set; } = true;
        public DateTime SyncedAtUtc { get; set; }
    }
}