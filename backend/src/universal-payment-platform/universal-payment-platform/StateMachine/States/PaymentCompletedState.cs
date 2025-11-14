using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using universal_payment_platform.Common;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.StateMachine.Core;

namespace universal_payment_platform.StateMachine.States
{
    public class PaymentCompletedState : StateBase<Payment>
    {
        public PaymentCompletedState() : base("Completed", "Payment has been successfully completed")
        {
            IsFinal = true;
        }

        protected override async Task<bool> OnCanEnterAsync(Payment context, IDictionary<string, object> parameters)
        {
            // Can enter completed state from processing state with successful result
            return context.Status == PaymentStatus.Processing &&
                   parameters?.ContainsKey("Success") == true &&
                   (bool)parameters["Success"] == true;
        }

        protected override async Task OnEnterStateAsync(Payment context, IDictionary<string, object> parameters)
        {
            context.Status = PaymentStatus.Completed;
            context.CompletedAt = DateTime.UtcNow;
            context.TransactionId = parameters?["TransactionId"] as string;

            // Update settlement information
            if (parameters?.ContainsKey("SettlementAmount") == true)
            {
                context.SettlementAmount = Convert.ToDecimal(parameters["SettlementAmount"]);
            }

            // Log state entry
            context.AddAuditTrail($"Entered {Name} state");

            await Task.CompletedTask;
        }

        protected override async Task OnExecuteStateAsync(Payment context)
        {
            // Execute completion tasks
            // Trigger webhooks
            // Update reporting
            // Handle post-payment actions

            await Task.CompletedTask;
        }
    }
}