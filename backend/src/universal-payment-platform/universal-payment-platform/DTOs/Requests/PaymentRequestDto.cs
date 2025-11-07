namespace universal_payment_platform.DTOs.Requests
{
    public class PaymentRequestDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Provider { get; set; }
        public string Description { get; set; }
        // Add any other fields you need from client request
    }
}
