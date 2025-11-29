using universal_payment_platform.Infrastructure.Email.Models;

namespace universal_payment_platform.Infrastructure.Email.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailMessage emailMessage);
        Task SendTemplatedEmailAsync<T>(string to, string subject, string templateName, T model) where T : class;
    }
}