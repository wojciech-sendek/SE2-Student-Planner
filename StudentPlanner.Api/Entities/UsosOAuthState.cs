namespace StudentPlanner.Api.Entities
{
    public class UsosOAuthState
    {
        public int Id { get; set; }

        public string State { get; set; } = null!;

        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public DateTime ExpiresAtUtc { get; set; }
    }
}