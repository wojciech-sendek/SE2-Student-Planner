namespace StudentPlanner.Api.Dtos.Usos
{
    public class UsosAuthorizationUrlResponseDto
    {
        public string? AuthorizationUrl { get; set; }
        public string? State { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}