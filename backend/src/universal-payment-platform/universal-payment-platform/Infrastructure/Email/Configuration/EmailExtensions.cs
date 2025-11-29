using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using universal_payment_platform.Infrastructure.Email.Services;

namespace universal_payment_platform.Infrastructure.Email.Configuration
{
    public static class EmailExtensions
    {
        public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure EmailSettings
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            // Register services with correct interfaces
            services.AddScoped<IEmailService, SmtpEmailService>();
            services.AddScoped<IEmailTemplateRenderer, EmailTemplateRenderer>();

            return services;
        }
    }
}