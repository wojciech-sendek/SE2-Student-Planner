namespace StudentPlanner.Api.Configurations
{
    public class SmtpOptions
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 1025;

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool EnableSsl { get; set; } = false;
        public string FromEmail { get; set; } = "noreply@studentplanner.pl";
        public string FromName { get; set; } = "Student Planner";
    }
}