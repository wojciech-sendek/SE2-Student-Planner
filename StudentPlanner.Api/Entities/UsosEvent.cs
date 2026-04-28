using System;

namespace StudentPlanner.Api.Entities
{
    public class UsosEvent : Event
    {
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public string? CourseId { get; set; }
        public string? LecturerName { get; set; }
        public string? Room { get; set; }
    }
}
