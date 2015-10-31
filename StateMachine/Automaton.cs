using System;
using System.Linq;
using System.Collections.Generic;

namespace StateMachine
{
    public class States : HashSet<State>
    {
        public override bool Equals(object obj)
        {
            var other = obj as States;
            if (other != null && other.Count != Count)
                return false;
            return other?.Intersect(this).Count() == Count;
        }

        public override int GetHashCode()
        {
            return this.Aggregate(Count, (current, state) => current ^ state.GetHashCode());
        }
    }

    public class StateNotFoundException : KeyNotFoundException
    {
        public StateNotFoundException()
        {
        }

        public StateNotFoundException(string message)
            : base(message)
        {
        }

        public StateNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Abstract state machine
    /// </summary>
    public abstract class Automaton<TEvent>
    {
        protected State StartState;
        protected States States;
        protected HashSet<TEvent> Events;

        public State Start
        {
            get { return StartState; }
            set
            {
                States.Add(StartState = value);
                StartState.Start = true;
            }
        }

        public State LastAdded { get; protected set; }

        protected Automaton()
        {
            States = new States();
            Events = new HashSet<TEvent>();

            States.Add(LastAdded = Start = new State());
        }

        public virtual void AddTransition(State from, State to, TEvent e)
        {
            if (!States.Contains(from))
                throw new StateNotFoundException();

            if (States.Add(to))
                LastAdded = to;
            Events.Add(e);
        }

        public abstract void Trigger(TEvent e);
    }

    /// <summary>
    /// Deterministic finite-state machine
    /// </summary>
    public class DFA<TEvent> : Automaton<TEvent>
    {
        private readonly Dictionary<KeyValuePair<State, TEvent>, State> _table;

        public State Current { get; private set; }

        public DFA()
        {
            _table = new Dictionary<KeyValuePair<State, TEvent>, State>();
            Current = StartState;
        }

        public override void AddTransition(State from, State to, TEvent e)
        {
            base.AddTransition(from, to, e);
            _table[new KeyValuePair<State, TEvent>(from, e)] = to;
        }

        public override void Trigger(TEvent e)
        {
            var key = new KeyValuePair<State, TEvent>(Current, e);
            if (!_table.ContainsKey(key))
                throw new StateNotFoundException();
            Current = _table[key];
        }
    }

    /// <summary>
    /// Non-deterministic finite-state machine
    /// </summary>
    public class NFA<TEvent> : Automaton<TEvent>
    {
        private readonly Dictionary<KeyValuePair<State, TEvent>, States> _table;
        private readonly Dictionary<State, States> _epsClosures;

        public States Current { get; private set; }

        public NFA()
        {
            _table = new Dictionary<KeyValuePair<State, TEvent>, States>();
            _epsClosures = new Dictionary<State, States> { { StartState, new States { StartState } } };
        }

        public override void AddTransition(State from, State to, TEvent e)
        {
            base.AddTransition(from, to, e);
            var key = new KeyValuePair<State, TEvent>(from, e);
            if (!_table.ContainsKey(key))
                _table.Add(key, new States());
            _table[key].Add(to);
        }

        /// <summary>
        /// Adds epsilon transition
        /// </summary>
        public void AddTransition(State from, State to)
        {
            if (!States.Contains(from))
                throw new StateNotFoundException();

            if (States.Add(to))
                LastAdded = to;

            if (!_epsClosures.ContainsKey(from))
                _epsClosures.Add(from, new States { from });

            if (!_epsClosures.ContainsKey(to))
                _epsClosures.Add(to, new States { to });

            foreach (var pair in _epsClosures)
                if (pair.Value.Contains(from))
                    pair.Value.UnionWith(_epsClosures[to]);
        }

        public void Initialize()
        {
            Current = new States();
            Current.UnionWith(_epsClosures[StartState]);
        }

        private States TransitState(States st, TEvent e)
        {
            var state = new States();
            foreach (State s in st)
            {
                States tmp;
                if (_table.TryGetValue(new KeyValuePair<State, TEvent>(s, e), out tmp))
                    state.UnionWith(tmp);
            }

            var epsilonState = new States();
            foreach (State s in state)
            {
                States tmp;
                if (_epsClosures.TryGetValue(s, out tmp))
                    epsilonState.UnionWith(tmp);
                epsilonState.Add(s);
            }

            return epsilonState;
        }

        public override void Trigger(TEvent e)
        {
            Current = TransitState(Current, e);
            if (Current.Count == 0)
                throw new StateNotFoundException();
        }

        public DFA<TEvent> ToDFA()
        {
            var a = new DFA<TEvent>();
            var q = new Queue<States>();
            var map = new Dictionary<States, State>();
            var used = new HashSet<States>();

            q.Enqueue(_epsClosures[StartState]);
            map.Add(q.Peek(), a.Start);

            while (q.Count > 0)
            {
                var state = q.Dequeue();
                if (!used.Add(state))
                    continue;

                foreach (TEvent e in Events)
                {
                    var next = TransitState(state, e);
                    if (next.Count == 0)
                        continue;
                    q.Enqueue(next);

                    var newState = map.ContainsKey(next)
                        ? map[next]
                        : new State
                        {
                            Start = next.FirstOrDefault(s => s.Start) != null,
                            Final = next.FirstOrDefault(s => s.Final) != null,
                            Name = string.Join(";", next.Select(s => s.Name).ToArray())
                        };

                    map[next] = newState;
                    a.AddTransition(map[state], newState, e);
                }
            }
            return a;
        }

        public void Merge(NFA<TEvent> other)
        {
            States.UnionWith(other.States);
            Events.UnionWith(other.Events);

            foreach (var item in other._epsClosures)
            {
                if (!_epsClosures.ContainsKey(item.Key))
                    _epsClosures.Add(item.Key, new States());
                _epsClosures[item.Key].UnionWith(item.Value);
            }

            foreach (var item in other._table)
            {
                if (!_table.ContainsKey(item.Key))
                    _table.Add(item.Key, new States());
                _table[item.Key].UnionWith(item.Value);
            }
        }
    }
}
