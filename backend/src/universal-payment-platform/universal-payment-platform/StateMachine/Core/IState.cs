using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace universal_payment_platform.StateMachine.Core
{
    /// <summary>
    /// Base interface for all states in the state machine
    /// </summary>
    public interface IState<TContext> where TContext : class
    {
        string Name { get; }
        string Description { get; }
        bool IsFinal { get; }
        bool IsInitial { get; }
        int HierarchyLevel { get; }
        IState<TContext> ParentState { get; }
        IReadOnlyCollection<IState<TContext>> SubStates { get; }

        Task<bool> CanEnterAsync(TContext context, IDictionary<string, object> parameters = null);
        Task<bool> CanExitAsync(TContext context, IDictionary<string, object> parameters = null);
        Task OnEnterAsync(TContext context, IDictionary<string, object> parameters = null);
        Task OnExitAsync(TContext context, IDictionary<string, object> parameters = null);
        Task OnExecuteAsync(TContext context);

        void AddSubState(IState<TContext> subState);
        void SetParent(IState<TContext> parent);
        bool IsInHierarchy(IState<TContext> state);
        IState<TContext> FindSubState(string stateName);
    }

    /// <summary>
    /// State metadata for persistence and analysis
    /// </summary>
    public interface IStateMetadata
    {
        DateTime CreatedAt { get; }
        DateTime? LastEnteredAt { get; }
        DateTime? LastExitedAt { get; }
        int EnterCount { get; }
        TimeSpan TotalTimeInState { get; }
        void RecordEntry();
        void RecordExit();
    }
}