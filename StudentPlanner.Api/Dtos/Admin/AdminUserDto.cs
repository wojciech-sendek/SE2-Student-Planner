namespace StudentPlanner.Api.Dtos.Admin
{
    public class AdminUserDto
    {
        public string Id { get; set; } = null!;
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
        public IReadOnlyList<int> FacultyIds { get; set; } = Array.Empty<int>();
        public IReadOnlyList<string> FacultyNames { get; set; } = Array.Empty<string>();
    }
}
