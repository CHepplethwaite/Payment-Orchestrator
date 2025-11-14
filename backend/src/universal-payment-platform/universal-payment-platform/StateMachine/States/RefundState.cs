using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using universal_payment_platform.Common;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.StateMachine.Core;

namespace universal_payment_platform.StateMachine.States
{
    public class RefundState : StateBase<Payment>
    {
        public RefundState() : base("Refunded", "Payment has been refunded")
        {
            IsFinal = true;
        }

        protected override async Task<bool> OnCanEnterAsync(Payment context, IDictionary<string, object> parameters)
        {
            // Can enter refunded state only from completed state
            return context.Status == PaymentStatus.Completed;
        }

        protected override async Task OnEnterStateAsync(Payment context, IDictionary<string, object> parameters)
        {
            context.Status = PaymentStatus.Refunded;
            context.RefundedAt = DateTime.UtcNow;

            // Record refund information
            if (parameters?.ContainsKey("RefundAmount") == true)
            {
                context.RefundAmount = Convert.ToDecimal(parameters["RefundAmount"]);
            }

            if (parameters?.ContainsKey("RefundReason") == true)
            {
                context.RefundReason = parameters["RefundReason"] as string;
            }

            // Log state entry
            context.AddAuditTrail($"Entered {Name} state");

            await Task.CompletedTask;
        }

        protected override async Task OnExecuteStateAsync(Payment context)
        {
            // Execute refund tasks
            // Process refund with provider
            // Update accounting
            // Notify customer

            await Task.CompletedTask;
        }
    }
}