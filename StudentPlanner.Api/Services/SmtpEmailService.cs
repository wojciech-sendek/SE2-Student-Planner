using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using StudentPlanner.Api.Configurations;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _smtpOptions;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IOptions<SmtpOptions> smtpOptions, ILogger<SmtpEmailService> logger)
        {
            _smtpOptions = smtpOptions.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_smtpOptions.Host, _smtpOptions.Port)
                {
                    EnableSsl = _smtpOptions.EnableSsl,
                    Credentials = string.IsNullOrWhiteSpace(_smtpOptions.Username) 
                        ? null 
                        : new NetworkCredential(_smtpOptions.Username, _smtpOptions.Password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpOptions.FromEmail, _smtpOptions.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                // We don't want to leak SMTP details or crash the flow if email sending fails,
                // but for a password reset flow, maybe we should inform the caller.
                // For now, rethrow to be handled by the service/controller.
                throw;
            }
        }
    }
}
