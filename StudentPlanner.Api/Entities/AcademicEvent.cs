namespace StudentPlanner.Api.Entities
{
    public class AcademicEvent : Event
    {
        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; } = null!;

        public ICollection<EventRequest> EventRequests { get; set; } = new List<EventRequest>();
    }
}
