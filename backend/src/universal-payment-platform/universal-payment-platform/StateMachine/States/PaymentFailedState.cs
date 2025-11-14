using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using universal_payment_platform.Common;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.StateMachine.Core;

namespace universal_payment_platform.StateMachine.States
{
    public class PaymentFailedState : StateBase<Payment>
    {
        public PaymentFailedState() : base("Failed", "Payment has failed")
        {
            IsFinal = true;
        }

        protected override async Task<bool> OnCanEnterAsync(Payment context, IDictionary<string, object> parameters)
        {
            // Can enter failed state from any state except completed or refunded
            return context.Status != PaymentStatus.Completed &&
                   context.Status != PaymentStatus.Refunded;
        }

        protected override async Task OnEnterStateAsync(Payment context, IDictionary<string, object> parameters)
        {
            context.Status = PaymentStatus.Failed;
            context.FailedAt = DateTime.UtcNow;

            // Record failure reason
            if (parameters?.ContainsKey("ErrorCode") == true)
            {
                context.ErrorCode = parameters["ErrorCode"] as string;
            }

            if (parameters?.ContainsKey("ErrorMessage") == true)
            {
                context.ErrorMessage = parameters["ErrorMessage"] as string;
            }

            // Log state entry
            context.AddAuditTrail($"Entered {Name} state: {context.ErrorMessage}");

            await Task.CompletedTask;
        }

        protected override async Task OnExecuteStateAsync(Payment context)
        {
            // Execute failure handling tasks
            // Notify stakeholders
            // Update failure analytics
            // Handle retry logic if applicable

            await Task.CompletedTask;
        }
    }
}