namespace StudentPlanner.Api.Dtos.Auth
{
    public class CurrentUserDto
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public IList<string> Roles { get; set; } = new List<string>();
    }
}