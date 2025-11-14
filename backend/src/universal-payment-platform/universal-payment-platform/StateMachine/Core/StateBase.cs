using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using universal_payment_platform.StateMachine.Core;

namespace universal_payment_platform.StateMachine.States
{
    /// <summary>
    /// Base implementation for all states with hierarchical support
    /// </summary>
    public abstract class StateBase<TContext> : IState<TContext>, IStateMetadata where TContext : class
    {
        private readonly List<IState<TContext>> _subStates;
        private DateTime _createdAt;
        private DateTime? _lastEnteredAt;
        private DateTime? _lastExitedAt;
        private int _enterCount;
        private TimeSpan _totalTimeInState;
        private DateTime? _currentEntryTime;

        public string Name { get; }
        public string Description { get; protected set; }
        public virtual bool IsFinal { get; protected set; }
        public virtual bool IsInitial { get; protected set; }
        public int HierarchyLevel { get; private set; }
        public IState<TContext> ParentState { get; private set; }
        public IReadOnlyCollection<IState<TContext>> SubStates => _subStates.AsReadOnly();

        // IStateMetadata implementation
        public DateTime CreatedAt => _createdAt;
        public DateTime? LastEnteredAt => _lastEnteredAt;
        public DateTime? LastExitedAt => _lastExitedAt;
        public int EnterCount => _enterCount;
        public TimeSpan TotalTimeInState => _totalTimeInState;

        protected StateBase(string name, string description = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? string.Empty;
            _subStates = new List<IState<TContext>>();
            _createdAt = DateTime.UtcNow;
            HierarchyLevel = 0;
        }

        public virtual async Task<bool> CanEnterAsync(TContext context, IDictionary<string, object> parameters = null)
        {
            // Check if all sub-states can be entered (for composite states)
            if (_subStates.Any())
            {
                foreach (var subState in _subStates.Where(s => s.IsInitial))
                {
                    if (!await subState.CanEnterAsync(context, parameters))
                        return false;
                }
            }

            return await OnCanEnterAsync(context, parameters);
        }

        public virtual async Task<bool> CanExitAsync(TContext context, IDictionary<string, object> parameters = null)
        {
            // Check if all active sub-states can exit
            if (_subStates.Any())
            {
                // This would need to track active sub-states in a real implementation
                foreach (var subState in _subStates)
                {
                    if (!await subState.CanExitAsync(context, parameters))
                        return false;
                }
            }

            return await OnCanExitAsync(context, parameters);
        }

        public virtual async Task OnEnterAsync(TContext context, IDictionary<string, object> parameters = null)
        {
            RecordEntry();

            await OnEnterStateAsync(context, parameters);

            // Enter initial sub-states for composite states
            if (_subStates.Any())
            {
                foreach (var subState in _subStates.Where(s => s.IsInitial))
                {
                    await subState.OnEnterAsync(context, parameters);
                }
            }
        }

        public virtual async Task OnExitAsync(TContext context, IDictionary<string, object> parameters = null)
        {
            RecordExit();

            // Exit all active sub-states
            if (_subStates.Any())
            {
                foreach (var subState in _subStates)
                {
                    await subState.OnExitAsync(context, parameters);
                }
            }

            await OnExitStateAsync(context, parameters);
        }

        public virtual async Task OnExecuteAsync(TContext context)
        {
            await OnExecuteStateAsync(context);

            // Execute active sub-states
            if (_subStates.Any())
            {
                foreach (var subState in _subStates)
                {
                    await subState.OnExecuteAsync(context);
                }
            }
        }

        public void AddSubState(IState<TContext> subState)
        {
            if (subState == null)
                throw new ArgumentNullException(nameof(subState));

            if (_subStates.Any(s => s.Name == subState.Name))
                throw new InvalidOperationException($"Sub-state '{subState.Name}' already exists");

            subState.SetParent(this);
            _subStates.Add(subState);
        }

        public void SetParent(IState<TContext> parent)
        {
            ParentState = parent;
            HierarchyLevel = parent?.HierarchyLevel + 1 ?? 0;

            // Update hierarchy level for all sub-states
            foreach (var subState in _subStates)
            {
                subState.SetParent(this);
            }
        }

        public bool IsInHierarchy(IState<TContext> state)
        {
            if (state == null)
                return false;

            // Check if this state is the same as the target state
            if (Equals(state))
                return true;

            // Check sub-states recursively
            return _subStates.Any(subState => subState.IsInHierarchy(state));
        }

        public IState<TContext> FindSubState(string stateName)
        {
            if (string.IsNullOrEmpty(stateName))
                return null;

            // Check direct sub-states
            var foundState = _subStates.FirstOrDefault(s => s.Name.Equals(stateName, StringComparison.OrdinalIgnoreCase));
            if (foundState != null)
                return foundState;

            // Search recursively in sub-states
            foreach (var subState in _subStates)
            {
                foundState = subState.FindSubState(stateName);
                if (foundState != null)
                    return foundState;
            }

            return null;
        }

        public void RecordEntry()
        {
            _enterCount++;
            _lastEnteredAt = DateTime.UtcNow;
            _currentEntryTime = _lastEnteredAt;
        }

        public void RecordExit()
        {
            _lastExitedAt = DateTime.UtcNow;
            if (_currentEntryTime.HasValue)
            {
                _totalTimeInState += _lastExitedAt.Value - _currentEntryTime.Value;
                _currentEntryTime = null;
            }
        }

        // Template methods for derived classes to override
        protected virtual Task<bool> OnCanEnterAsync(TContext context, IDictionary<string, object> parameters) => Task.FromResult(true);
        protected virtual Task<bool> OnCanExitAsync(TContext context, IDictionary<string, object> parameters) => Task.FromResult(true);
        protected virtual Task OnEnterStateAsync(TContext context, IDictionary<string, object> parameters) => Task.CompletedTask;
        protected virtual Task OnExitStateAsync(TContext context, IDictionary<string, object> parameters) => Task.CompletedTask;
        protected virtual Task OnExecuteStateAsync(TContext context) => Task.CompletedTask;

        public override string ToString() => $"{Name} (Level: {HierarchyLevel})";

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var subState in _subStates)
                {
                    if (subState is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                _subStates.Clear();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}