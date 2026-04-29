namespace StudentPlanner.Api.Configurations
{
    public class UsosOptions
    {
        public const string SectionName = "Usos";

        public string AuthorizationEndpoint { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = string.Empty;
        public string ScheduleEndpoint { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;

        public bool UseMockScheduleWhenNotConfigured { get; set; } = true;
    }
}