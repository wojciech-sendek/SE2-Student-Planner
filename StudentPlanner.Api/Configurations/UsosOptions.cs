namespace StudentPlanner.Api.Configurations
{
    public class UsosOptions
    {
        public const string SectionName = "Usos";

        public string AuthorizationEndpoint { get; set; } = null!;
        public string TokenEndpoint { get; set; } = null!;
        public string ScheduleEndpoint { get; set; } = null!;

        public string ClientId { get; set; } = null!;
        public string ClientSecret { get; set; } = null!;
        public string RedirectUri { get; set; } = null!;

        public string Scope { get; set; } = "schedule";
        public int OAuthStateLifetimeMinutes { get; set; } = 10;
    }
}