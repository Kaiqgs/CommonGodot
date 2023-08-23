using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FSM
{
    public struct UpdateArgs
    {
        public float Delta;
        public string Action;
    }

    public interface IState
    {
        string Name { get; }
        void Update(UpdateArgs update);
        void Enter(Dictionary<string, object> context, string action);
        bool Predicate(UpdateArgs update);
        Dictionary<string, object> Exit();
    }

    public abstract class State : Godot.Object, IState
    {
        protected enum Transit
        {
            Immediate,
            Never,
            OnInput
        }

        protected virtual string _name { get; set; }
        protected Dictionary<string, object> context = new Dictionary<string, object>();
        protected virtual Transit Behaviour { get; set; } = Transit.Never;
        public virtual string Name
        {
            get => this._name ?? this.GetType().Name;
        }

        public State()
        {
            this._name = this.GetType().Name;
        }

        public virtual void Enter(Dictionary<string, object> context, string action)
        {
            this.context = context;
        }

        public virtual void Update(UpdateArgs update) { }

        public virtual Dictionary<string, object> Exit()
        {
            var context = this.context;
            this.context = null;
            return context;
        }

        public virtual bool Predicate(UpdateArgs update)
        {
            if (this.Behaviour == Transit.Never)
            {
                return false;
            }
            return this.Behaviour == Transit.Immediate || !string.IsNullOrEmpty(update.Action);
        }

        protected T getCtx<T>(string key)
        {
            object output = default(T);
            if (this.context?.TryGetValue(key, out output) ?? false)
            {
                return (T)output;
            }
            return (T)output;
        }
    }

    public class FiniteStateMachine
    {
        private Dictionary<string, IState> _states;
        private Dictionary<string, HashSet<string>> _transitions;
        private Dictionary<string, Func<bool>> _conditions;

        private IState _currentState;
        private HashSet<string> _alphabet;
        private string _initialState;
        private bool _ranBefore = false;

        public FiniteStateMachine(string initial, string[] alphabet)
        {
            this._states = new Dictionary<string, IState>();
            this._alphabet = new HashSet<string>(alphabet);
            this._transitions = new Dictionary<string, HashSet<string>>();
            this._conditions = new Dictionary<string, Func<bool>>();
            this._initialState = initial;
        }

        private string UnisonTransition(string fromName, string toName)
        {
            return fromName + "x" + toName;
        }

        public void AddTransition(string fromName, string toName, Func<bool> predicator = null)
        {
            string transitionName = UnisonTransition(fromName, toName);
            if (this._transitions.TryGetValue(fromName, out HashSet<string> toStates))
            {
                toStates.Add(toName);
            }
            else
            {
                this._transitions.Add(fromName, new HashSet<string>() { toName });
            }
            this._conditions[transitionName] = predicator;
        }

        public void AddState(IState state)
        {
            GD.Print("Adding state: " + state.Name);
            GD.Print("Initial state: " + this._initialState);
            this._states.Add(state.Name, state);
            if (state.Name == this._initialState)
            {
                this._setState(this._initialState, null);
            }
        }

        private void _setState(string name, string action)
        {
            if (!this._states.ContainsKey(name))
            {
                throw new Exception("State not found: " + name);
            }
            var ctx = _currentState?.Exit() ?? new Dictionary<string, object>();
            _currentState = _states[name];
            _currentState.Enter(ctx, action);
            GD.Print("Moving to state: ", name);
        }

        public IState GetCurrentState()
        {
            return _currentState;
        }

        public IState GetState(string name)
        {
            return this._states[name];
        }

        public void Update(float delta, string action)
        {
            var args = new UpdateArgs() { Delta = delta, Action = action };
            if (
                this._currentState != null && this._transitions.ContainsKey(this._currentState.Name)
            )
            {
                var transitions = this._transitions[this._currentState.Name];

                foreach (var nextState in transitions)
                {
                    // var transitionName = UnisonTransition(this._currentState.Name, nextState);
                    var transitionName = "";
                    if (
                        this._currentState.Predicate(args)
                        || (
                            this._conditions.ContainsKey(transitionName)
                            && this._conditions[transitionName]()
                        )
                    )
                    {
                        GD.Print("Updating FSM: ", this._currentState?.Name, " Action: ", action);
                        this._setState(nextState, action);
                        return;
                    }
                }
            }
            this._currentState?.Update(args);
        }
    }
}
