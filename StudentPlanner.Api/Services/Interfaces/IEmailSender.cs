namespace StudentPlanner.Api.Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendPasswordResetTokenAsync(string email, string token);
    }
}