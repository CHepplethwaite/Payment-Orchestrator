namespace universal_payment_platform.Services.Interfaces.Models
{
    public class PaymentResponse
    {
        public string TransactionId { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? ProviderReference { get; set; }
    }
}