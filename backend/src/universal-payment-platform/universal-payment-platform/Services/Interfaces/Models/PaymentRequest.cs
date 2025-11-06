namespace universal_payment_platform.Services.Interfaces.Models
{
    public class PaymentRequest
    {
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "ZMW";
        public string? Description { get; set; }
        public string? CustomerId { get; set; }
        public string? Provider { get; set; }
        // Add other payment-specific properties
    }
}