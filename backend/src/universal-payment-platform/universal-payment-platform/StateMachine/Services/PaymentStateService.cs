using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.StateMachine.Core;
using universal_payment_platform.StateMachine.States;
using universal_payment_platform.StateMachine.Transitions;

namespace universal_payment_platform.StateMachine.Services
{
    public interface IPaymentStateService
    {
        Task<bool> InitializePaymentStateMachineAsync(Payment payment);
        Task<TransitionResult> ProcessPaymentAsync(Payment payment);
        Task<TransitionResult> CompletePaymentAsync(Payment payment, string transactionId);
        Task<TransitionResult> FailPaymentAsync(Payment payment, string errorCode, string errorMessage);
        Task<TransitionResult> RefundPaymentAsync(Payment payment, decimal refundAmount, string reason);
        Task<bool> CanRefundAsync(Payment payment);
        Task<IState<Payment>> GetCurrentStateAsync(Payment payment);
        Task<IReadOnlyCollection<IStateHistory<Payment>>> GetStateHistoryAsync(Payment payment);
    }

    public class PaymentStateService : IPaymentStateService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentStateService> _logger;
        private readonly Dictionary<string, IStateMachine<Payment>> _stateMachines;

        public PaymentStateService(IServiceProvider serviceProvider, ILogger<PaymentStateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _stateMachines = new Dictionary<string, IStateMachine<Payment>>();
        }

        public async Task<bool> InitializePaymentStateMachineAsync(Payment payment)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            try
            {
                var stateMachine = CreatePaymentStateMachine();
                var initialized = await stateMachine.InitializeAsync(payment);

                if (initialized)
                {
                    _stateMachines[payment.Id.ToString()] = stateMachine;
                    _logger.LogInformation("Payment state machine initialized for payment {PaymentId}", payment.Id);
                }

                return initialized;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize payment state machine for payment {PaymentId}", payment.Id);
                return false;
            }
        }

        public async Task<TransitionResult> ProcessPaymentAsync(Payment payment)
        {
            return await TransitionToStateAsync(payment, "Processing", new Dictionary<string, object>
            {
                ["ProcessedAt"] = DateTime.UtcNow
            });
        }

        public async Task<TransitionResult> CompletePaymentAsync(Payment payment, string transactionId)
        {
            return await TransitionToStateAsync(payment, "Completed", new Dictionary<string, object>
            {
                ["Success"] = true,
                ["TransactionId"] = transactionId,
                ["CompletedAt"] = DateTime.UtcNow
            });
        }

        public async Task<TransitionResult> FailPaymentAsync(Payment payment, string errorCode, string errorMessage)
        {
            return await TransitionToStateAsync(payment, "Failed", new Dictionary<string, object>
            {
                ["ErrorCode"] = errorCode,
                ["ErrorMessage"] = errorMessage,
                ["FailedAt"] = DateTime.UtcNow
            });
        }

        public async Task<TransitionResult> RefundPaymentAsync(Payment payment, decimal refundAmount, string reason)
        {
            return await TransitionToStateAsync(payment, "Refunded", new Dictionary<string, object>
            {
                ["RefundAmount"] = refundAmount,
                ["RefundReason"] = reason,
                ["RefundedAt"] = DateTime.UtcNow
            });
        }

        public async Task<bool> CanRefundAsync(Payment payment)
        {
            var stateMachine = GetStateMachineForPayment(payment);
            if (stateMachine == null)
                return false;

            return await stateMachine.CanTransitionToAsync("Refunded");
        }

        public async Task<IState<Payment>> GetCurrentStateAsync(Payment payment)
        {
            var stateMachine = GetStateMachineForPayment(payment);
            return stateMachine?.CurrentState;
        }

        public async Task<IReadOnlyCollection<IStateHistory<Payment>>> GetStateHistoryAsync(Payment payment)
        {
            var stateMachine = GetStateMachineForPayment(payment);
            return stateMachine?.History ?? new List<IStateHistory<Payment>>().AsReadOnly();
        }

        private async Task<TransitionResult> TransitionToStateAsync(Payment payment, string stateName, IDictionary<string, object> parameters = null)
        {
            var stateMachine = GetStateMachineForPayment(payment);
            if (stateMachine == null)
            {
                return TransitionResult.Failure("State machine not initialized for payment");
            }

            try
            {
                var result = await stateMachine.TransitionToAsync(stateName, parameters);

                if (result.IsSuccessful)
                {
                    _logger.LogInformation("Payment {PaymentId} transitioned to {StateName}", payment.Id, stateName);
                }
                else
                {
                    _logger.LogWarning("Failed to transition payment {PaymentId} to {StateName}: {Error}",
                        payment.Id, stateName, result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transitioning payment {PaymentId} to {StateName}", payment.Id, stateName);
                return TransitionResult.Failure($"Transition error: {ex.Message}", ex);
            }
        }

        private IStateMachine<Payment> GetStateMachineForPayment(Payment payment)
        {
            if (_stateMachines.TryGetValue(payment.Id.ToString(), out var stateMachine))
            {
                return stateMachine;
            }

            // Try to initialize if not found
            var initialized = InitializePaymentStateMachineAsync(payment).GetAwaiter().GetResult();
            if (initialized)
            {
                return _stateMachines[payment.Id.ToString()];
            }

            return null;
        }

        private IStateMachine<Payment> CreatePaymentStateMachine()
        {
            var logger = _serviceProvider.GetService<ILogger<StateMachine<Payment>>>();
            var stateMachine = new StateMachine<Payment>("PaymentStateMachine", version: new Version(1, 0, 0), logger: logger);

            // Create states
            var initiatedState = new PaymentInitiatedState();
            var processingState = new PaymentProcessingState();
            var completedState = new PaymentCompletedState();
            var failedState = new PaymentFailedState();
            var refundState = new RefundState();

            // Add states to state machine
            stateMachine.AddState(initiatedState);
            stateMachine.AddState(processingState);
            stateMachine.AddState(completedState);
            stateMachine.AddState(failedState);
            stateMachine.AddState(refundState);

            // Define transitions
            var transitions = new[]
            {
                // Initiated -> Processing
                new PaymentStateTransition(
                    "StartProcessing",
                    initiatedState,
                    processingState,
                    "PROCESS_PAYMENT",
                    guard: CanProcessPaymentGuard,
                    action: ProcessPaymentAction),

                // Processing -> Completed
                new PaymentStateTransition(
                    "CompletePayment",
                    processingState,
                    completedState,
                    "PAYMENT_SUCCESS",
                    guard: CanCompletePaymentGuard,
                    action: CompletePaymentAction),

                // Processing -> Failed
                new PaymentStateTransition(
                    "FailPayment",
                    processingState,
                    failedState,
                    "PAYMENT_FAILED",
                    guard: CanFailPaymentGuard,
                    action: FailPaymentAction),

                // Completed -> Refunded
                new PaymentStateTransition(
                    "RefundPayment",
                    completedState,
                    refundState,
                    "REFUND_PAYMENT",
                    guard: CanRefundPaymentGuard,
                    action: RefundPaymentAction),

                // Any state -> Failed (with appropriate guards)
                new PaymentStateTransition(
                    "ForceFail",
                    initiatedState,
                    failedState,
                    "FORCE_FAIL",
                    guard: CanForceFailGuard,
                    action: ForceFailAction)
            };

            foreach (var transition in transitions)
            {
                stateMachine.AddTransition(transition);
            }

            // Subscribe to events
            stateMachine.StateChanged += OnPaymentStateChanged;
            stateMachine.ErrorOccurred += OnPaymentStateMachineError;

            return stateMachine;
        }

        // Guard conditions
        private static async Task<bool> CanProcessPaymentGuard(Payment payment, IDictionary<string, object> parameters)
        {
            return payment.Amount > 0 &&
                   !string.IsNullOrEmpty(payment.Currency) &&
                   payment.MerchantId != Guid.Empty;
        }

        private static async Task<bool> CanCompletePaymentGuard(Payment payment, IDictionary<string, object> parameters)
        {
            return parameters?.ContainsKey("TransactionId") == true &&
                   !string.IsNullOrEmpty(parameters["TransactionId"] as string);
        }

        private static async Task<bool> CanFailPaymentGuard(Payment payment, IDictionary<string, object> parameters)
        {
            return parameters?.ContainsKey("ErrorCode") == true &&
                   !string.IsNullOrEmpty(parameters["ErrorCode"] as string);
        }

        private static async Task<bool> CanRefundPaymentGuard(Payment payment, IDictionary<string, object> parameters)
        {
            var refundAmount = parameters?["RefundAmount"] as decimal? ?? 0;
            return refundAmount > 0 && refundAmount <= payment.Amount;
        }

        private static async Task<bool> CanForceFailGuard(Payment payment, IDictionary<string, object> parameters)
        {
            // Can force fail only if payment has been stuck for too long
            var maxProcessingTime = TimeSpan.FromHours(24);
            return payment.CreatedAt < DateTime.UtcNow - maxProcessingTime;
        }

        // Action methods
        private static async Task<TransitionResult> ProcessPaymentAction(Payment payment, IDictionary<string, object> parameters)
        {
            // Here you would integrate with actual payment processing
            payment.ProcessingStartedAt = DateTime.UtcNow;
            return TransitionResult.Success();
        }

        private static async Task<TransitionResult> CompletePaymentAction(Payment payment, IDictionary<string, object> parameters)
        {
            payment.TransactionId = parameters["TransactionId"] as string;
            payment.CompletedAt = DateTime.UtcNow;
            return TransitionResult.Success();
        }

        private static async Task<TransitionResult> FailPaymentAction(Payment payment, IDictionary<string, object> parameters)
        {
            payment.ErrorCode = parameters["ErrorCode"] as string;
            payment.ErrorMessage = parameters["ErrorMessage"] as string;
            payment.FailedAt = DateTime.UtcNow;
            return TransitionResult.Success();
        }

        private static async Task<TransitionResult> RefundPaymentAction(Payment payment, IDictionary<string, object> parameters)
        {
            payment.RefundAmount = (decimal)(parameters["RefundAmount"] ?? 0m);
            payment.RefundReason = parameters["RefundReason"] as string;
            payment.RefundedAt = DateTime.UtcNow;
            return TransitionResult.Success();
        }

        private static async Task<TransitionResult> ForceFailAction(Payment payment, IDictionary<string, object> parameters)
        {
            payment.ErrorCode = "TIMEOUT";
            payment.ErrorMessage = "Payment processing timed out";
            payment.FailedAt = DateTime.UtcNow;
            return TransitionResult.Success();
        }

        private void OnPaymentStateChanged(object sender, StateChangedEventArgs<Payment> e)
        {
            _logger.LogInformation(
                "Payment state changed from {OldState} to {NewState}",
                e.OldState.Name,
                e.NewState.Name);
        }

        private void OnPaymentStateMachineError(object sender, StateMachineErrorEventArgs<Payment> e)
        {
            _logger.LogError(
                e.Exception,
                "State machine error during {Operation}",
                e.Operation);
        }
    }
}