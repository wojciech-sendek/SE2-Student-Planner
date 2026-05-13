namespace StudentPlanner.Api.Services
{
    public class ManagerFacultyNotAssignedException : Exception
    {
        public ManagerFacultyNotAssignedException()
            : base("Manager has no assigned faculty.")
        {
        }
    }

    public class ManagerFacultyAccessDeniedException : Exception
    {
        public ManagerFacultyAccessDeniedException()
            : base("Manager cannot access events outside their assigned faculty.")
        {
        }
    }

    public class AcademicEventNotFoundException : Exception
    {
        public AcademicEventNotFoundException(int eventId)
            : base($"Academic event with id {eventId} was not found.")
        {
        }
    }
}
