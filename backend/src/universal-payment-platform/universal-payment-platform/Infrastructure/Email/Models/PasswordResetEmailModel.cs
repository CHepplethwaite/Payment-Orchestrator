namespace universal_payment_platform.Infrastructure.Email.Models
{
    public class PasswordResetEmailModel
    {
        public string Name { get; set; } = string.Empty;
        public string ResetUrl { get; set; } = string.Empty;
        public string ResetCode { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 30;
    }
}