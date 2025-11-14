using System;
using universal_payment_platform.Data.Entities;

namespace universal_payment_platform.StateMachine.Events
{
    public class RefundRequestedEvent
    {
        public Guid PaymentId { get; }
        public decimal RefundAmount { get; }
        public string RefundReason { get; }
        public DateTime RequestedAt { get; }
        public string RequestedBy { get; }

        public RefundRequestedEvent(Guid paymentId, decimal refundAmount, string refundReason, string requestedBy)
        {
            PaymentId = paymentId;
            RefundAmount = refundAmount;
            RefundReason = refundReason;
            RequestedAt = DateTime.UtcNow;
            RequestedBy = requestedBy;
        }
    }
}