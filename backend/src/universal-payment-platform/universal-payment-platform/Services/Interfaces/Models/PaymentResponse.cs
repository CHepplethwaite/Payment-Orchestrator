namespace universal_payment_platform.Services.Interfaces.Models
{
    public class PaymentResponse
    {
        public bool IsSuccess { get; set; }
        public string TransactionId { get; set; }
        public string Reference { get; set; }
        public string Message { get; set; }
        public string StatusCode { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; }
    }
}