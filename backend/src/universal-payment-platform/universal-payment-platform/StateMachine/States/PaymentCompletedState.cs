using universal_payment_platform.Common;
using universal_payment_platform.Data.Entities;

namespace universal_payment_platform.StateMachine.States
{
    public class PaymentCompletedState : StateBase<Payment>
    {
        public PaymentCompletedState()
            : base("Completed", "Payment has been successfully completed")
        {
            IsFinal = true;
        }

        protected override Task<bool> OnCanEnterAsync(
            Payment context,
            IDictionary<string, object>? parameters = null)
        {
            if (parameters == null)
                return Task.FromResult(false);

            var success = parameters.TryGetValue("Success", out var successObj) &&
                          successObj is bool flag &&
                          flag;

            var validPreviousState =
                context.Status == PaymentStatus.Processing ||
                context.Status == PaymentStatus.Authorized ||
                context.Status == PaymentStatus.Pending;

            return Task.FromResult(success && validPreviousState);
        }

        protected override Task OnEnterStateAsync(
            Payment context,
            IDictionary<string, object>? parameters = null)
        {
            context.Status = PaymentStatus.Completed;
            context.CompletedAt = DateTime.UtcNow;

            if (parameters?.TryGetValue("TransactionId", out var tx) == true)
                context.TransactionId = tx as string;

            if (parameters?.TryGetValue("SettlementAmount", out var amt) == true)
                context.SettlementAmount = Convert.ToDecimal(amt);

            context.AddAuditTrail($"Entered {Name} state");

            return Task.CompletedTask;
        }

        protected override Task OnExecuteStateAsync(Payment context)
        {
            // webhook notifications
            // settlement updates
            // reporting
            // async post-processing

            return Task.CompletedTask;
        }
    }
}
