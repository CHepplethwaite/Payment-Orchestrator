using MediatR;
using universal_payment_platform.DTOs.Responses;

namespace universal_payment_platform.DTOs.Requests
{
    public class PaymentRequest : IRequest<PaymentResponse>
    {
        public required string Provider { get; set; }              // e.g. "AirtelMoney", "MTNMoney"
        public required string UserId { get; set; }                // User initiating the payment

        // The actual payment details
        public required string TransactionId { get; set; }         // External transaction reference
        public required decimal Amount { get; set; }               // Amount to pay
        public required string Currency { get; set; }              // e.g. "ZMW"
        public required string Description { get; set; }           // Optional
        public required string AccountNumber { get; set; }         // Recipient or payer account
    }
}
