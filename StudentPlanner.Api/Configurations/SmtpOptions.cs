namespace StudentPlanner.Api.Configurations
{
    public class SmtpOptions
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; } = null!;
        public int Port { get; set; } = 587;
        public bool EnableSsl { get; set; } = true;

        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string From { get; set; } = null!;
    }
}
