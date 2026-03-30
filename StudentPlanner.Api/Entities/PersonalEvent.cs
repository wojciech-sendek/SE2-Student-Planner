namespace StudentPlanner.Api.Entities
{
    public class PersonalEvent : Event
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}