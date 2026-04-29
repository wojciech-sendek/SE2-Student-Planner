namespace StudentPlanner.Api.Dtos.Auth
{
    public class RegisterResponseDto
    {
        public string Message { get; set; } = "Registration successful.";
        public string UserId { get; set; } = null!;
        public string? UsosAuthorizationUrl { get; set; }
        public string? UsosState { get; set; }
    }
}