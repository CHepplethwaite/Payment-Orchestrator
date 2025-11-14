using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using universal_payment_platform.StateMachine.Core;

namespace universal_payment_platform.StateMachine.Core
{
    /// <summary>
    /// Comprehensive state machine implementation with hierarchical support
    /// </summary>
    public class StateMachine<TContext> : IStateMachine<TContext> where TContext : class
    {
        private readonly ILogger<StateMachine<TContext>> _logger;
        private readonly Dictionary<string, IState<TContext>> _states;
        private readonly Dictionary<string, ITransition<TContext>> _transitions;
        private readonly List<IStateHistory<TContext>> _history;
        private readonly Dictionary<string, IStateMachine<TContext>> _subMachines;
        private readonly StateMachineConfiguration _configuration;
        private readonly object _lockObject = new object();

        public string MachineId { get; }
        public string Name { get; }
        public Version Version { get; }
        public IState<TContext> CurrentState { get; private set; }
        public IState<TContext> PreviousState { get; private set; }
        public IState<TContext> InitialState { get; private set; }
        public TContext Context { get; private set; }

        public IReadOnlyCollection<IState<TContext>> States => _states.Values.ToList().AsReadOnly();
        public IReadOnlyCollection<ITransition<TContext>> Transitions => _transitions.Values.ToList().AsReadOnly();
        public IReadOnlyCollection<IStateHistory<TContext>> History => _history.AsReadOnly();

        public event EventHandler<StateChangedEventArgs<TContext>> StateChanging;
        public event EventHandler<StateChangedEventArgs<TContext>> StateChanged;
        public event EventHandler<TransitionEventArgs<TContext>> TransitionExecuting;
        public event EventHandler<TransitionEventArgs<TContext>> TransitionExecuted;
        public event EventHandler<StateMachineErrorEventArgs<TContext>> ErrorOccurred;

        public StateMachine(string name, string machineId = null, Version version = null, ILogger<StateMachine<TContext>> logger = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            MachineId = machineId ?? Guid.NewGuid().ToString();
            Version = version ?? new Version(1, 0, 0);
            _logger = logger;

            _states = new Dictionary<string, IState<TContext>>(StringComparer.OrdinalIgnoreCase);
            _transitions = new Dictionary<string, ITransition<TContext>>(StringComparer.OrdinalIgnoreCase);
            _history = new List<IStateHistory<TContext>>();
            _subMachines = new Dictionary<string, IStateMachine<TContext>>(StringComparer.OrdinalIgnoreCase);
            _configuration = new StateMachineConfiguration();
        }

        public async Task<bool> InitializeAsync(TContext context, string initialStateName = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            Context = context;

            if (_states.Count == 0)
            {
                _logger?.LogWarning("State machine '{MachineName}' has no states defined", Name);
                return false;
            }

            // Find initial state
            if (!string.IsNullOrEmpty(initialStateName))
            {
                InitialState = FindState(initialStateName);
                if (InitialState == null)
                {
                    _logger?.LogError("Initial state '{InitialStateName}' not found in state machine '{MachineName}'",
                        initialStateName, Name);
                    return false;
                }
            }
            else
            {
                InitialState = _states.Values.FirstOrDefault(s => s.IsInitial);
                if (InitialState == null)
                {
                    InitialState = _states.Values.First();
                    _logger?.LogInformation("No initial state specified, using first state: {StateName}", InitialState.Name);
                }
            }

            if (!await InitialState.CanEnterAsync(Context))
            {
                _logger?.LogError("Cannot enter initial state '{InitialStateName}' in state machine '{MachineName}'",
                    InitialState.Name, Name);
                return false;
            }

            CurrentState = InitialState;
            await CurrentState.OnEnterAsync(Context);

            _logger?.LogInformation("State machine '{MachineName}' initialized with state '{InitialStateName}'",
                Name, CurrentState.Name);

            return true;
        }

        public async Task<TransitionResult> TransitionToAsync(string stateName, IDictionary<string, object> parameters = null)
        {
            if (string.IsNullOrEmpty(stateName))
                throw new ArgumentNullException(nameof(stateName));

            lock (_lockObject)
            {
                if (CurrentState?.IsFinal == true)
                {
                    return TransitionResult.Failure($"Cannot transition from final state '{CurrentState.Name}'");
                }
            }

            var targetState = FindState(stateName);
            if (targetState == null)
            {
                return TransitionResult.Failure($"Target state '{stateName}' not found");
            }

            // Find valid transition
            var transition = FindTransition(CurrentState, targetState);
            if (transition == null)
            {
                return TransitionResult.Failure($"No valid transition found from '{CurrentState.Name}' to '{targetState.Name}'");
            }

            return await ExecuteTransitionAsync(transition, parameters);
        }

        public async Task<bool> CanTransitionToAsync(string stateName, IDictionary<string, object> parameters = null)
        {
            if (string.IsNullOrEmpty(stateName))
                return false;

            var targetState = FindState(stateName);
            if (targetState == null)
                return false;

            var transition = FindTransition(CurrentState, targetState);
            if (transition == null)
                return false;

            return await transition.CanExecuteAsync(Context, parameters);
        }

        public void AddState(IState<TContext> state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            if (_states.ContainsKey(state.Name))
                throw new InvalidOperationException($"State '{state.Name}' already exists");

            _states[state.Name] = state;

            // If this is the first state and no initial state is set, mark it as initial
            if (_states.Count == 1 && InitialState == null && !state.IsFinal)
            {
                // Use reflection to set IsInitial if possible, or handle through constructor
            }

            _logger?.LogDebug("State '{StateName}' added to state machine '{MachineName}'", state.Name, Name);
        }

        public void AddTransition(ITransition<TContext> transition)
        {
            if (transition == null)
                throw new ArgumentNullException(nameof(transition));

            var transitionKey = $"{transition.SourceState.Name}->{transition.TargetState.Name}";
            if (_transitions.ContainsKey(transitionKey))
                throw new InvalidOperationException($"Transition '{transitionKey}' already exists");

            _transitions[transitionKey] = transition;
            _logger?.LogDebug("Transition '{TransitionKey}' added to state machine '{MachineName}'", transitionKey, Name);
        }

        public void RemoveState(string stateName)
        {
            if (string.IsNullOrEmpty(stateName))
                throw new ArgumentNullException(nameof(stateName));

            if (_states.ContainsKey(stateName))
            {
                // Remove all transitions involving this state
                var transitionsToRemove = _transitions
                    .Where(t => t.Value.SourceState.Name.Equals(stateName) || t.Value.TargetState.Name.Equals(stateName))
                    .Select(t => t.Key)
                    .ToList();

                foreach (var transitionKey in transitionsToRemove)
                {
                    _transitions.Remove(transitionKey);
                }

                _states.Remove(stateName);
                _logger?.LogDebug("State '{StateName}' removed from state machine '{MachineName}'", stateName, Name);
            }
        }

        public void RemoveTransition(string transitionName)
        {
            if (string.IsNullOrEmpty(transitionName))
                throw new ArgumentNullException(nameof(transitionName));

            if (_transitions.ContainsKey(transitionName))
            {
                _transitions.Remove(transitionName);
                _logger?.LogDebug("Transition '{TransitionName}' removed from state machine '{MachineName}'", transitionName, Name);
            }
        }

        public bool IsInState(string stateName)
        {
            if (string.IsNullOrEmpty(stateName))
                return false;

            return CurrentState?.Name.Equals(stateName, StringComparison.OrdinalIgnoreCase) == true ||
                   CurrentState?.IsInHierarchy(FindState(stateName)) == true;
        }

        public bool IsInState(IState<TContext> state)
        {
            return CurrentState == state || CurrentState?.IsInHierarchy(state) == true;
        }

        public IState<TContext> FindState(string stateName)
        {
            if (string.IsNullOrEmpty(stateName))
                return null;

            if (_states.TryGetValue(stateName, out var state))
                return state;

            // Search in sub-states
            foreach (var existingState in _states.Values)
            {
                var foundState = existingState.FindSubState(stateName);
                if (foundState != null)
                    return foundState;
            }

            return null;
        }

        public async Task<bool> FireEventAsync(string eventName, IDictionary<string, object> parameters = null)
        {
            if (string.IsNullOrEmpty(eventName))
                return false;

            var transitions = _transitions.Values
                .Where(t => t.TriggerEvent?.Equals(eventName, StringComparison.OrdinalIgnoreCase) == true &&
                           t.SourceState == CurrentState)
                .OrderByDescending(t => t.Priority)
                .ToList();

            foreach (var transition in transitions)
            {
                if (await transition.CanExecuteAsync(Context, parameters))
                {
                    var result = await ExecuteTransitionAsync(transition, parameters);
                    return result.IsSuccessful;
                }
            }

            return false;
        }

        public IStateMachine<TContext> GetSubMachine(string stateName)
        {
            _subMachines.TryGetValue(stateName, out var subMachine);
            return subMachine;
        }

        public bool HasSubMachine(string stateName) => _subMachines.ContainsKey(stateName);

        public async Task<StateMachineSnapshot<TContext>> CreateSnapshotAsync()
        {
            return new StateMachineSnapshot<TContext>
            {
                MachineId = MachineId,
                CurrentStateName = CurrentState?.Name,
                PreviousStateName = PreviousState?.Name,
                SnapshotTime = DateTime.UtcNow,
                ContextData = await SerializeContextAsync(),
                History = _history.Select(h => new StateHistoryRecord
                {
                    FromState = h.FromState.Name,
                    ToState = h.ToState.Name,
                    TransitionTime = h.TransitionTime,
                    WasSuccessful = h.WasSuccessful
                }).ToList(),
                Version = Version.ToString()
            };
        }

        public async Task<bool> RestoreFromSnapshotAsync(StateMachineSnapshot<TContext> snapshot)
        {
            if (snapshot == null)
                return false;

            try
            {
                await DeserializeContextAsync(snapshot.ContextData);

                if (!string.IsNullOrEmpty(snapshot.CurrentStateName))
                {
                    var state = FindState(snapshot.CurrentStateName);
                    if (state != null)
                    {
                        CurrentState = state;
                    }
                }

                if (!string.IsNullOrEmpty(snapshot.PreviousStateName))
                {
                    PreviousState = FindState(snapshot.PreviousStateName);
                }

                _history.Clear();
                // Restore history if needed

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to restore state machine from snapshot");
                OnErrorOccurred(ex, "RestoreFromSnapshot");
                return false;
            }
        }

        public async Task<StateMachineValidationResult> ValidateAsync()
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            // Check for unreachable states
            var reachableStates = GetReachableStates();
            var unreachableStates = _states.Values.Where(s => !reachableStates.Contains(s) && !s.IsInitial).ToList();
            foreach (var state in unreachableStates)
            {
                warnings.Add($"State '{state.Name}' is unreachable from initial state");
            }

            // Check for states without outgoing transitions (except final states)
            var deadEndStates = _states.Values
                .Where(s => !s.IsFinal && !_transitions.Values.Any(t => t.SourceState == s))
                .ToList();

            foreach (var state in deadEndStates)
            {
                errors.Add($"State '{state.Name}' has no outgoing transitions and is not a final state");
            }

            // Validate all transitions
            foreach (var transition in _transitions.Values)
            {
                try
                {
                    if (!await transition.ValidateAsync(Context))
                    {
                        warnings.Add($"Transition '{transition.Name}' validation failed");
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Transition '{transition.Name}' validation error: {ex.Message}");
                }
            }

            return errors.Count == 0 && warnings.Count == 0
                ? StateMachineValidationResult.Valid()
                : new StateMachineValidationResult
                {
                    IsValid = errors.Count == 0,
                    Errors = errors,
                    Warnings = warnings
                };
        }

        private async Task<TransitionResult> ExecuteTransitionAsync(ITransition<TContext> transition, IDictionary<string, object> parameters)
        {
            var startTime = DateTime.UtcNow;

            try
            {
                OnTransitionExecuting(transition, parameters);

                // Check if we can exit current state
                if (!await CurrentState.CanExitAsync(Context, parameters))
                {
                    return TransitionResult.Failure($"Cannot exit current state '{CurrentState.Name}'");
                }

                // Check if we can enter target state
                if (!await transition.TargetState.CanEnterAsync(Context, parameters))
                {
                    return TransitionResult.Failure($"Cannot enter target state '{transition.TargetState.Name}'");
                }

                OnStateChanging(CurrentState, transition.TargetState, parameters);

                // Execute transition
                var transitionResult = await transition.ExecuteAsync(Context, parameters);
                if (!transitionResult.IsSuccessful)
                {
                    return transitionResult;
                }

                // Exit current state
                await CurrentState.OnExitAsync(Context, parameters);

                // Update state history
                PreviousState = CurrentState;
                var historyEntry = new StateHistory<TContext>(
                    PreviousState,
                    transition.TargetState,
                    DateTime.UtcNow,
                    parameters);

                _history.Add(historyEntry);

                // Limit history size
                if (_history.Count > _configuration.MaxHistorySize)
                {
                    _history.RemoveAt(0);
                }

                // Enter new state
                CurrentState = transition.TargetState;
                await CurrentState.OnEnterAsync(Context, parameters);

                OnStateChanged(PreviousState, CurrentState, parameters);
                OnTransitionExecuted(transition, parameters);

                var duration = DateTime.UtcNow - startTime;
                _logger?.LogInformation("Transition from '{FromState}' to '{ToState}' completed in {Duration}ms",
                    PreviousState.Name, CurrentState.Name, duration.TotalMilliseconds);

                return TransitionResult.Success();

            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Transition from '{FromState}' to '{ToState}' failed",
                    CurrentState.Name, transition.TargetState.Name);

                OnErrorOccurred(ex, "ExecuteTransition");
                return TransitionResult.Failure($"Transition failed: {ex.Message}", ex);
            }
        }

        private ITransition<TContext> FindTransition(IState<TContext> fromState, IState<TContext> toState)
        {
            var directTransitionKey = $"{fromState.Name}->{toState.Name}";
            if (_transitions.TryGetValue(directTransitionKey, out var transition))
                return transition;

            // Check for wildcard transitions
            return _transitions.Values
                .FirstOrDefault(t => t.SourceState == fromState && t.TargetState == toState);
        }

        private HashSet<IState<TContext>> GetReachableStates()
        {
            var reachable = new HashSet<IState<TContext>>();
            if (InitialState == null)
                return reachable;

            var queue = new Queue<IState<TContext>>();
            queue.Enqueue(InitialState);
            reachable.Add(InitialState);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var outgoingTransitions = _transitions.Values.Where(t => t.SourceState == current);

                foreach (var transition in outgoingTransitions)
                {
                    if (reachable.Add(transition.TargetState))
                    {
                        queue.Enqueue(transition.TargetState);
                    }
                }
            }

            return reachable;
        }

        protected virtual void OnStateChanging(IState<TContext> oldState, IState<TContext> newState, IDictionary<string, object> parameters)
        {
            StateChanging?.Invoke(this, new StateChangedEventArgs<TContext>(oldState, newState, parameters));
        }

        protected virtual void OnStateChanged(IState<TContext> oldState, IState<TContext> newState, IDictionary<string, object> parameters)
        {
            StateChanged?.Invoke(this, new StateChangedEventArgs<TContext>(oldState, newState, parameters));
        }

        protected virtual void OnTransitionExecuting(ITransition<TContext> transition, IDictionary<string, object> parameters)
        {
            TransitionExecuting?.Invoke(this, new TransitionEventArgs<TContext>(transition, parameters));
        }

        protected virtual void OnTransitionExecuted(ITransition<TContext> transition, IDictionary<string, object> parameters)
        {
            TransitionExecuted?.Invoke(this, new TransitionEventArgs<TContext>(transition, parameters));
        }

        protected virtual void OnErrorOccurred(Exception exception, string operation)
        {
            ErrorOccurred?.Invoke(this, new StateMachineErrorEventArgs<TContext>(exception, operation));
        }

        private Task<IDictionary<string, object>> SerializeContextAsync()
        {
            // Implementation depends on context type
            // This should be overridden in derived classes for specific context types
            return Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>());
        }

        private Task DeserializeContextAsync(IDictionary<string, object> contextData)
        {
            // Implementation depends on context type
            // This should be overridden in derived classes for specific context types
            return Task.CompletedTask;
        }
    }

    internal class StateMachineConfiguration
    {
        public int MaxHistorySize { get; set; } = 1000;
        public bool EnableLogging { get; set; } = true;
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
    }

    internal class StateHistory<TContext> : IStateHistory<TContext> where TContext : class
    {
        public string Id { get; }
        public IState<TContext> FromState { get; }
        public IState<TContext> ToState { get; }
        public DateTime TransitionTime { get; }
        public TimeSpan DurationInPreviousState { get; }
        public IDictionary<string, object> Parameters { get; }
        public bool WasSuccessful { get; }
        public string ErrorMessage { get; }

        public StateHistory(IState<TContext> fromState, IState<TContext> toState, DateTime transitionTime, IDictionary<string, object> parameters)
        {
            Id = Guid.NewGuid().ToString();
            FromState = fromState;
            ToState = toState;
            TransitionTime = transitionTime;
            Parameters = parameters ?? new Dictionary<string, object>();
            WasSuccessful = true;
        }
    }
}