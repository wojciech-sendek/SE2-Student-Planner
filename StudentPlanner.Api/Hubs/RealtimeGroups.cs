namespace StudentPlanner.Api.Hubs
{
    public static class RealtimeGroups
    {
        public static string User(string userId) => $"user:{userId}";
        public static string Role(string role) => $"role:{role}";
        public static string Faculty(int facultyId) => $"faculty:{facultyId}";
        public static string AcademicEvent(int academicEventId) => $"academic-event:{academicEventId}";
    }
}
