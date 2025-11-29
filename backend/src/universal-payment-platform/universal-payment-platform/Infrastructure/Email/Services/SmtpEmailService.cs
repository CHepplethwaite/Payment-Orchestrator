using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using universal_payment_platform.Infrastructure.Email.Configuration;
using universal_payment_platform.Infrastructure.Email.Models;

namespace universal_payment_platform.Infrastructure.Email.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly IEmailTemplateRenderer _templateRenderer;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(
            IOptions<EmailSettings> emailSettings,
            IEmailTemplateRenderer templateRenderer,
            ILogger<SmtpEmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _templateRenderer = templateRenderer;
            _logger = logger;
        }

        public async Task SendEmailAsync(EmailMessage emailMessage)
        {
            if (string.IsNullOrWhiteSpace(emailMessage.To))
                throw new ArgumentException("Recipient email cannot be empty", nameof(emailMessage.To));

            try
            {
                using var client = CreateSmtpClient();
                using var mailMessage = CreateMailMessage(emailMessage);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {Recipient}", emailMessage.To);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}", emailMessage.To);
                throw;
            }
        }

        public async Task SendTemplatedEmailAsync<T>(string to, string subject, string templateName, T model) where T : class
        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("Recipient email cannot be empty", nameof(to));

            var body = await _templateRenderer.RenderTemplateAsync(templateName, model);

            var emailMessage = new EmailMessage
            {
                To = to,
                Subject = subject,
                Body = body,
                IsHtml = true,
                From = _emailSettings.SenderEmail,  // <- Use SenderEmail from config
            };

            await SendEmailAsync(emailMessage);
        }

        private SmtpClient CreateSmtpClient()
        {
            return new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port)
            {
                EnableSsl = _emailSettings.EnableSsl,
                UseDefaultCredentials = _emailSettings.UseDefaultCredentials,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network
            };
        }

        private MailMessage CreateMailMessage(EmailMessage emailMessage)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = emailMessage.Subject,
                Body = emailMessage.Body,
                IsBodyHtml = emailMessage.IsHtml
            };

            mailMessage.To.Add(emailMessage.To);

            foreach (var attachment in emailMessage.Attachments)
            {
                var stream = new MemoryStream(attachment.Content);
                mailMessage.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
            }

            return mailMessage;
        }
    }
}
