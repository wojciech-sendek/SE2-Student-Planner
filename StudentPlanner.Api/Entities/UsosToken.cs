namespace StudentPlanner.Api.Entities
{
    public class UsosToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public string AccessTokenSecret { get; set; } = null!;
    }
}
