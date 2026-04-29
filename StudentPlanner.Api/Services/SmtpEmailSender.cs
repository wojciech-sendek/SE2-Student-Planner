using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using StudentPlanner.Api.Configurations;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions _options;

        public SmtpEmailSender(IOptions<SmtpOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendPasswordResetTokenAsync(string email, string token)
        {
            using var message = new MailMessage
            {
                From = new MailAddress(_options.From),
                Subject = "Student Planner password reset token",
                Body = $"""
                        Your Student Planner password reset token:

                        {token}

                        This token is time-limited. If you did not request a password reset, ignore this email.
                        """,
                IsBodyHtml = false
            };

            message.To.Add(email);

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.EnableSsl,
                Credentials = new NetworkCredential(_options.Username, _options.Password)
            };

            await client.SendMailAsync(message);
        }
    }
}