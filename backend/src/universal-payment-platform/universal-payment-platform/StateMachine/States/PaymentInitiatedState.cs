using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using universal_payment_platform.Common;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.StateMachine.Core;

namespace universal_payment_platform.StateMachine.States
{
    public class PaymentInitiatedState : StateBase<Payment>
    {
        public PaymentInitiatedState() : base("Initiated", "Payment has been initiated and is awaiting processing")
        {
            IsInitial = true;
        }

        protected override async Task<bool> OnCanEnterAsync(Payment context, IDictionary<string, object> parameters)
        {
            // Can enter initiated state if payment is newly created
            return context.Status == PaymentStatus.Pending ||
                   context.Status == PaymentStatus.Initiated;
        }

        protected override async Task OnEnterStateAsync(Payment context, IDictionary<string, object> parameters)
        {
            context.Status = PaymentStatus.Initiated;
            context.InitiatedAt = DateTime.UtcNow;

            // Log state entry
            context.AddAuditTrail($"Entered {Name} state");

            await Task.CompletedTask;
        }

        protected override async Task OnExecuteStateAsync(Payment context)
        {
            // Perform initialization tasks
            // Validate payment parameters
            // Check merchant limits
            // Prepare for provider selection

            await Task.CompletedTask;
        }
    }
}