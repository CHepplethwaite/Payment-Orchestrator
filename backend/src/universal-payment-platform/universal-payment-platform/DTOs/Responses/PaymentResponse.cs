// File: universal-payment-platform/DTOs/Responses/PaymentResponse.cs

using universal_payment_platform.Common; // Needed for the PaymentStatus enum

namespace universal_payment_platform.DTOs.Responses
{
    /// <summary>
    /// Represents the response object returned by the payment adapter and the CQRS command handler.
    /// </summary>
    public class PaymentResponse
    {
        // Properties used in your handler's initialization and updates:
        public required string TransactionId { get; set; }  // Maps to the external transaction ID
        public PaymentStatus Status { get; set; }  // FIXES missing Status (e.g., Completed, Failed)
        public required string Message { get; set; }        // FIXES missing Message (e.g., "Success", "Card declined")

        // Other crucial properties for a universal payment platform response:
        public required string ProviderReference { get; set; } // The ID assigned by the external provider (e.g., Stripe ID)
        public decimal Amount { get; set; }
        public required string Currency { get; set; }
    }
}