using System;
using universal_payment_platform.Data.Entities;

namespace universal_payment_platform.StateMachine.Events
{
    public class PaymentFailedEvent
    {
        public Guid PaymentId { get; }
        public decimal Amount { get; }
        public string Currency { get; }
        public string ErrorCode { get; }
        public string ErrorMessage { get; }
        public DateTime FailedAt { get; }

        public PaymentFailedEvent(Payment payment)
        {
            PaymentId = payment.Id;
            Amount = payment.Amount;
            Currency = payment.Currency;
            ErrorCode = payment.ErrorCode;
            ErrorMessage = payment.ErrorMessage;
            FailedAt = payment.FailedAt ?? DateTime.UtcNow;
        }
    }
}