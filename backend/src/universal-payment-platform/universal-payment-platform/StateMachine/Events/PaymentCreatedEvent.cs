using System;
using universal_payment_platform.Data.Entities;

namespace universal_payment_platform.StateMachine.Events
{
    public class PaymentCreatedEvent
    {
        public Guid PaymentId { get; }
        public decimal Amount { get; }
        public string Currency { get; }
        public Guid MerchantId { get; }
        public DateTime CreatedAt { get; }

        public PaymentCreatedEvent(Payment payment)
        {
            PaymentId = payment.Id;
            Amount = payment.Amount;
            Currency = payment.Currency;
            MerchantId = payment.MerchantId;
            CreatedAt = payment.CreatedAt;
        }
    }
}