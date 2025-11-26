using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using universal_payment_platform.Common;
using universal_payment_platform.Data.Entities;
using universal_payment_platform.StateMachine.Core;

namespace universal_payment_platform.StateMachine.Transitions
{
    public class PaymentStateTransition : ITransition<Payment>
    {
        private readonly Func<Payment, IDictionary<string, object>, Task<bool>> _guard;
        private readonly Func<Payment, IDictionary<string, object>, Task<TransitionResult>> _action;

        public string Name { get; }
        public string Description { get; }
        public IState<Payment> SourceState { get; }
        public IState<Payment> TargetState { get; }
        public string TriggerEvent { get; }
        public int Priority { get; }

        public PaymentStateTransition(
            string name,
            IState<Payment> sourceState,
            IState<Payment> targetState,
            string triggerEvent = null,
            int priority = 1,
            string description = null,
            Func<Payment, IDictionary<string, object>, Task<bool>> guard = null,
            Func<Payment, IDictionary<string, object>, Task<TransitionResult>> action = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            SourceState = sourceState ?? throw new ArgumentNullException(nameof(sourceState));
            TargetState = targetState ?? throw new ArgumentNullException(nameof(targetState));
            TriggerEvent = triggerEvent;
            Priority = priority;
            Description = description ?? $"{sourceState.Name} -> {targetState.Name}";
            _guard = guard;
            _action = action;
        }

        public async Task<bool> CanExecuteAsync(Payment context, IDictionary<string, object> parameters = null)
        {
            try
            {
                // Check basic conditions
                if (context == null)
                    return false;

                // Check if source state matches current context state
                if (context.Status.ToString() != SourceState.Name)
                    return false;

                // Execute custom guard condition
                if (_guard != null)
                {
                    return await _guard(context, parameters);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<TransitionResult> ExecuteAsync(Payment context, IDictionary<string, object> parameters = null)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                // Execute transition action if provided
                if (_action != null)
                {
                    var actionResult = await _action(context, parameters);
                    if (!actionResult.IsSuccessful)
                    {
                        return actionResult;
                    }
                }

                // Update payment status to target state
                context.Status = Enum.Parse<PaymentStatus>(TargetState.Name);

                var duration = DateTime.UtcNow - startTime;
                return TransitionResult.Success();

            }
            catch (Exception ex)
            {
                return TransitionResult.Failure($"Transition execution failed: {ex.Message}", ex);
            }
        }

        public async Task<bool> ValidateAsync(Payment context)
        {
            // Validate that the transition makes sense in the current context
            if (SourceState.IsFinal && !TargetState.IsFinal)
            {
                return false; // Cannot transition from final to non-final state
            }

            if (SourceState == TargetState)
            {
                return false; // Self-transitions should be explicit
            }

            return await Task.FromResult(true);
        }

        public override string ToString() => $"{Name} ({SourceState.Name} -> {TargetState.Name})";
    }

    /// <summary>
    /// Builder for creating transitions fluently
    /// </summary>
    public class TransitionBuilder<TContext> where TContext : class
    {
        private string _name;
        private IState<TContext> _sourceState;
        private IState<TContext> _targetState;
        private string _triggerEvent;
        private int _priority = 1;
        private string _description;
        private Func<TContext, IDictionary<string, object>, Task<bool>> _guard;
        private Func<TContext, IDictionary<string, object>, Task<TransitionResult>> _action;

        public TransitionBuilder<TContext> WithName(string name)
        {
            _name = name;
            return this;
        }

        public TransitionBuilder<TContext> FromState(IState<TContext> sourceState)
        {
            _sourceState = sourceState;
            return this;
        }

        public TransitionBuilder<TContext> ToState(IState<TContext> targetState)
        {
            _targetState = targetState;
            return this;
        }

        public TransitionBuilder<TContext> WithTrigger(string triggerEvent)
        {
            _triggerEvent = triggerEvent;
            return this;
        }

        public TransitionBuilder<TContext> WithPriority(int priority)
        {
            _priority = priority;
            return this;
        }

        public TransitionBuilder<TContext> WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public TransitionBuilder<TContext> WithGuard(Func<TContext, IDictionary<string, object>, Task<bool>> guard)
        {
            _guard = guard;
            return this;
        }

        public TransitionBuilder<TContext> WithAction(Func<TContext, IDictionary<string, object>, Task<TransitionResult>> action)
        {
            _action = action;
            return this;
        }

        public ITransition<TContext> Build()
        {
            return new CustomTransition<TContext>(
                _name,
                _sourceState,
                _targetState,
                _triggerEvent,
                _priority,
                _description,
                _guard,
                _action);
        }
    }

    internal class CustomTransition<TContext> : ITransition<TContext> where TContext : class
    {
        private readonly Func<TContext, IDictionary<string, object>, Task<bool>> _guard;
        private readonly Func<TContext, IDictionary<string, object>, Task<TransitionResult>> _action;

        public string Name { get; }
        public string Description { get; }
        public IState<TContext> SourceState { get; }
        public IState<TContext> TargetState { get; }
        public string TriggerEvent { get; }
        public int Priority { get; }

        public CustomTransition(
            string name,
            IState<TContext> sourceState,
            IState<TContext> targetState,
            string triggerEvent,
            int priority,
            string description,
            Func<TContext, IDictionary<string, object>, Task<bool>> guard,
            Func<TContext, IDictionary<string, object>, Task<TransitionResult>> action)
        {
            Name = name;
            SourceState = sourceState;
            TargetState = targetState;
            TriggerEvent = triggerEvent;
            Priority = priority;
            Description = description;
            _guard = guard;
            _action = action;
        }

        public Task<bool> CanExecuteAsync(TContext context, IDictionary<string, object>? parameters = null)
        {
            return _guard?.Invoke(context, parameters) ?? Task.FromResult(true);
        }

        public Task<TransitionResult> ExecuteAsync(TContext context, IDictionary<string, object>? parameters = null)
        {
            return _action?.Invoke(context, parameters) ?? Task.FromResult(TransitionResult.Success());
        }

        public Task<bool> ValidateAsync(TContext context)
        {
            return Task.FromResult(SourceState != null && TargetState != null && !string.IsNullOrEmpty(Name));
        }
    }
}