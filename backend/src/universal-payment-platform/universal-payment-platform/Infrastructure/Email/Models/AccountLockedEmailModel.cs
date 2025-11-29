namespace universal_payment_platform.Infrastructure.Email.Models
{
    public class AccountLockedEmailModel
    {
        public string Name { get; set; } = string.Empty;
        public DateTime LockedAt { get; set; }
        public string UnlockUrl { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = string.Empty;
    }
}