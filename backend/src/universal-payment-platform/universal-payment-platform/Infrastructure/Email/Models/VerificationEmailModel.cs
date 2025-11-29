namespace universal_payment_platform.Infrastructure.Email.Models
{
    public class VerificationEmailModel
    {
        public string Name { get; set; } = string.Empty;
        public string VerificationUrl { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
        public int ExpiryHours { get; set; } = 24;
    }
}