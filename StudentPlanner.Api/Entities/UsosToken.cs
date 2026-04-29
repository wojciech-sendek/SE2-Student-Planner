namespace StudentPlanner.Api.Entities
{
    public class UsosToken
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public string AccessTokenEncrypted { get; set; } = null!;
        public string RefreshTokenEncrypted { get; set; } = null!;

        public DateTime AccessTokenExpiresAtUtc { get; set; }

        public string? TokenType { get; set; }
        public string? Scope { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}