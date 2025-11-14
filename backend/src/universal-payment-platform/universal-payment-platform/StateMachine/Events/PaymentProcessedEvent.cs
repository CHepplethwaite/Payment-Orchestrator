using System;
using universal_payment_platform.Data.Entities;

namespace universal_payment_platform.StateMachine.Events
{
    public class PaymentProcessedEvent
    {
        public Guid PaymentId { get; }
        public string TransactionId { get; }
        public decimal Amount { get; }
        public string Currency { get; }
        public DateTime ProcessedAt { get; }
        public string ProviderName { get; }

        public PaymentProcessedEvent(Payment payment, string transactionId, string providerName)
        {
            PaymentId = payment.Id;
            TransactionId = transactionId;
            Amount = payment.Amount;
            Currency = payment.Currency;
            ProcessedAt = DateTime.UtcNow;
            ProviderName = providerName;
        }
    }
}