namespace universal_payment_platform.Services.Interfaces.Models
{
    public class PaymentRequest
    {
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string CustomerMSISDN { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, string> AdditionalParameters { get; set; } = new Dictionary<string, string>();

        // Add this property so the system knows which provider to use
        public string Provider { get; set; } = string.Empty;
    }
}
