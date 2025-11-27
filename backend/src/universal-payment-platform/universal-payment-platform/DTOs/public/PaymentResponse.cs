using universal_payment_platform.Common;

namespace universal_payment_platform.DTOs.@public
{
    /// <summary>
    /// Represents the response object returned by the payment adapter and the CQRS command handler.
    /// </summary>
    public class PaymentResponse
    {
        // Properties used in your handler's initialization and updates:
        public required string TransactionId { get; set; }  // Maps to the external transaction ID
        public PaymentStatus Status { get; set; } 
        public required string Message { get; set; }      

        // Other crucial properties for a universal payment platform response:
        public required string ProviderReference { get; set; } 
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
    }
}