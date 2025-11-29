
namespace universal_payment_platform.Infrastructure.Email.Services
{
    public interface IEmailTemplateRenderer
    {
        Task<string> RenderTemplateAsync<T>(string templateName, T model) where T : class;
    }

    public class EmailTemplateRenderer(IHostEnvironment environment, ILogger<EmailTemplateRenderer> logger) : IEmailTemplateRenderer
    {
        private readonly IHostEnvironment _environment = environment;
        private readonly ILogger<EmailTemplateRenderer> _logger = logger;

        public async Task<string> RenderTemplateAsync<T>(string templateName, T model) where T : class
        {
            try
            {
                var templatePath = GetTemplatePath(templateName);

                if (!File.Exists(templatePath))
                {
                    // Fallback to a simple template if file doesn't exist
                    return CreateFallbackTemplate(templateName, model);
                }

                var templateContent = await File.ReadAllTextAsync(templatePath);
                return ReplaceTemplatePlaceholders(templateContent, model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering email template {TemplateName}", templateName);
                return CreateFallbackTemplate(templateName, model);
            }
        }

        private string GetTemplatePath(string templateName)
        {
            var templateDirectory = Path.Combine(_environment.ContentRootPath, "Infrastructure", "Email", "Templates");
            return Path.Combine(templateDirectory, $"{templateName}.html");
        }

        private static string ReplaceTemplatePlaceholders<T>(string template, T model) where T : class
        {
            var result = template;
            var properties = typeof(T).GetProperties();

            foreach (var property in properties)
            {
                var placeholder = $"{{{{{property.Name}}}}}";
                var value = property.GetValue(model)?.ToString() ?? string.Empty;
                result = result.Replace(placeholder, value);
            }

            return result;
        }

        private static string CreateFallbackTemplate<T>(string templateName, T model)
        {
            var properties = typeof(T).GetProperties();
            var content = $"<h1>{templateName}</h1><div>";

            foreach (var property in properties)
            {
                var value = property.GetValue(model)?.ToString() ?? string.Empty;
                content += $"<p><strong>{property.Name}:</strong> {value}</p>";
            }

            content += "</div>";
            return content;
        }
    }
}