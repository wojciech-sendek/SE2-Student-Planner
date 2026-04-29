namespace StudentPlanner.Api.Entities
{
    public class UsosEvent : Event
    {
        public string ExternalId { get; set; } = null!;
        public string? Room { get; set; }
        public string? Teacher { get; set; }
        public DateTime SyncedAtUtc { get; set; }

        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}