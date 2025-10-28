namespace universal_payment_platform.Services.Interfaces.Models
{
    public class PaymentRequest
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
        public string CustomerMSISDN { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public Dictionary<string, string> AdditionalParameters { get; set; }

        // Add this property so the system knows which provider to use
        public string Provider { get; set; }
    }
}
