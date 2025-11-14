using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using universal_payment_platform.Common;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.StateMachine.Core;

namespace universal_payment_platform.StateMachine.States
{
    public class PaymentProcessingState : StateBase<Payment>
    {
        public PaymentProcessingState() : base("Processing", "Payment is being processed by the payment provider")
        {
        }

        protected override async Task<bool> OnCanEnterAsync(Payment context, IDictionary<string, object> parameters)
        {
            // Can enter processing state from initiated state
            return context.Status == PaymentStatus.Initiated ||
                   context.Status == PaymentStatus.Processing;
        }

        protected override async Task OnEnterStateAsync(Payment context, IDictionary<string, object> parameters)
        {
            context.Status = PaymentStatus.Processing;
            context.ProcessingStartedAt = DateTime.UtcNow;

            // Log state entry
            context.AddAuditTrail($"Entered {Name} state");

            await Task.CompletedTask;
        }

        protected override async Task OnExecuteStateAsync(Payment context)
        {
            // Execute payment processing logic
            // Communicate with payment provider
            // Handle timeouts and retries

            await Task.CompletedTask;
        }

        protected override async Task<bool> OnCanExitAsync(Payment context, IDictionary<string, object> parameters)
        {
            // Can only exit if payment has been processed or failed
            return context.Status == PaymentStatus.Completed ||
                   context.Status == PaymentStatus.Failed ||
                   context.Status == PaymentStatus.RequiresAction;
        }
    }
}