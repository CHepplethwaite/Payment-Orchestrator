using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace universal_payment_platform.StateMachine.Core
{
    /// <summary>
    /// Main state machine interface with comprehensive functionality
    /// </summary>
    public interface IStateMachine<TContext> where TContext : class
    {
        string MachineId { get; }
        string Name { get; }
        Version Version { get; }

        IState<TContext> CurrentState { get; }
        IState<TContext> PreviousState { get; }
        IState<TContext> InitialState { get; }
        TContext Context { get; }

        IReadOnlyCollection<IState<TContext>> States { get; }
        IReadOnlyCollection<ITransition<TContext>> Transitions { get; }
        IReadOnlyCollection<IStateHistory<TContext>> History { get; }

        event EventHandler<StateChangedEventArgs<TContext>> StateChanging;
        event EventHandler<StateChangedEventArgs<TContext>> StateChanged;
        event EventHandler<TransitionEventArgs<TContext>> TransitionExecuting;
        event EventHandler<TransitionEventArgs<TContext>> TransitionExecuted;
        event EventHandler<StateMachineErrorEventArgs<TContext>> ErrorOccurred;

        Task<bool> InitializeAsync(TContext context, string initialStateName = null);
        Task<TransitionResult> TransitionToAsync(string stateName, IDictionary<string, object> parameters = null);
        Task<bool> CanTransitionToAsync(string stateName, IDictionary<string, object> parameters = null);

        void AddState(IState<TContext> state);
        void AddTransition(ITransition<TContext> transition);
        void RemoveState(string stateName);
        void RemoveTransition(string transitionName);

        bool IsInState(string stateName);
        bool IsInState(IState<TContext> state);
        IState<TContext> FindState(string stateName);

        Task<bool> FireEventAsync(string eventName, IDictionary<string, object> parameters = null);

        // Hierarchical state machine support
        IStateMachine<TContext> GetSubMachine(string stateName);
        bool HasSubMachine(string stateName);

        // Persistence
        Task<StateMachineSnapshot<TContext>> CreateSnapshotAsync();
        Task<bool> RestoreFromSnapshotAsync(StateMachineSnapshot<TContext> snapshot);

        // Validation
        Task<StateMachineValidationResult> ValidateAsync();
    }

    /// <summary>
    /// Transition interface
    /// </summary>
    public interface ITransition<TContext> where TContext : class
    {
        string Name { get; }
        string Description { get; }
        IState<TContext> SourceState { get; }
        IState<TContext> TargetState { get; }
        string TriggerEvent { get; }
        int Priority { get; }

        Task<bool> CanExecuteAsync(TContext context, IDictionary<string, object> parameters = null);
        Task<TransitionResult> ExecuteAsync(TContext context, IDictionary<string, object> parameters = null);
        Task<bool> ValidateAsync(TContext context);
    }

    /// <summary>
    /// State history tracking
    /// </summary>
    public interface IStateHistory<TContext> where TContext : class
    {
        string Id { get; }
        IState<TContext> FromState { get; }
        IState<TContext> ToState { get; }
        DateTime TransitionTime { get; }
        TimeSpan DurationInPreviousState { get; }
        IDictionary<string, object> Parameters { get; }
        bool WasSuccessful { get; }
        string ErrorMessage { get; }
    }

    // Event args classes
    public class StateChangedEventArgs<TContext> : EventArgs where TContext : class
    {
        public IState<TContext> OldState { get; }
        public IState<TContext> NewState { get; }
        public DateTime ChangeTime { get; }
        public IDictionary<string, object> Parameters { get; }

        public StateChangedEventArgs(IState<TContext> oldState, IState<TContext> newState, IDictionary<string, object> parameters)
        {
            OldState = oldState;
            NewState = newState;
            ChangeTime = DateTime.UtcNow;
            Parameters = parameters;
        }
    }

    public class TransitionEventArgs<TContext> : EventArgs where TContext : class
    {
        public ITransition<TContext> Transition { get; }
        public DateTime ExecutionTime { get; }
        public IDictionary<string, object> Parameters { get; }

        public TransitionEventArgs(ITransition<TContext> transition, IDictionary<string, object> parameters)
        {
            Transition = transition;
            ExecutionTime = DateTime.UtcNow;
            Parameters = parameters;
        }
    }

    public class StateMachineErrorEventArgs<TContext> : EventArgs where TContext : class
    {
        public Exception Exception { get; }
        public string Operation { get; }
        public DateTime ErrorTime { get; }

        public StateMachineErrorEventArgs(Exception exception, string operation)
        {
            Exception = exception;
            Operation = operation;
            ErrorTime = DateTime.UtcNow;
        }
    }

    // Result classes
    public class TransitionResult
    {
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
        public TimeSpan Duration { get; set; }
        public IDictionary<string, object> AdditionalData { get; set; }

        public static TransitionResult Success() => new TransitionResult { IsSuccessful = true };
        public static TransitionResult Failure(string error, Exception ex = null) =>
            new TransitionResult { IsSuccessful = false, ErrorMessage = error, Exception = ex };
    }

    public class StateMachineValidationResult
    {
        public bool IsValid { get; set; }
        public IReadOnlyCollection<string> Errors { get; set; }
        public IReadOnlyCollection<string> Warnings { get; set; }

        public static StateMachineValidationResult Valid() =>
            new StateMachineValidationResult { IsValid = true, Errors = Array.Empty<string>(), Warnings = Array.Empty<string>() };

        public static StateMachineValidationResult Invalid(params string[] errors) =>
            new StateMachineValidationResult { IsValid = false, Errors = errors, Warnings = Array.Empty<string>() };
    }

    // Snapshot for persistence
    public class StateMachineSnapshot<TContext> where TContext : class
    {
        public string MachineId { get; set; }
        public string CurrentStateName { get; set; }
        public string PreviousStateName { get; set; }
        public DateTime SnapshotTime { get; set; }
        public IDictionary<string, object> ContextData { get; set; }
        public IReadOnlyCollection<StateHistoryRecord> History { get; set; }
        public string Version { get; set; }
    }

    public class StateHistoryRecord
    {
        public string FromState { get; set; }
        public string ToState { get; set; }
        public DateTime TransitionTime { get; set; }
        public bool WasSuccessful { get; set; }
    }
}