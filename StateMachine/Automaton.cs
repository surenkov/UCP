using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Configuration;

namespace StateMachine
{
    using States = HashSet<State>;

    public class StateNotFoundException : KeyNotFoundException
    {
        public StateNotFoundException()
        {
        }

        public StateNotFoundException(string message) : base(message)
        {
        }

        public StateNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Abstract state machine
    /// </summary>
    public abstract class Automaton<Event>
    {
        protected State _start;
        protected States _states;
        protected HashSet<Event> _events;

        public State Start
        {
            get { return _start; }
            set
            {
                _states.Add(_start = value);
                _start.Start = true;
            }
        }

        public State LastAdded { get; protected set; }

        protected Automaton()
        {
            _states = new HashSet<State>();
            _events = new HashSet<Event>();

            _states.Add(LastAdded = Start = new State());
        }

        public virtual void AddTransition(State from, State to, Event e)
        {
            if (!_states.Contains(from))
                throw new StateNotFoundException();

            if (_states.Add(to))
                LastAdded = to;
            _events.Add(e);
        }

        public abstract void Trigger(Event e);
    }

    /// <summary>
    /// Deterministic finite-state machine
    /// </summary>
    public class DFA<Event> : Automaton<Event>
    {
        private readonly Dictionary<KeyValuePair<State, Event>, State> _table;

        public State Current { get; private set; }

        public DFA()
        {
            _table = new Dictionary<KeyValuePair<State, Event>, State>();
            Current = _start;
        }

        public override void AddTransition(State from, State to, Event e)
        {
            base.AddTransition(from, to, e);
            _table.Add(new KeyValuePair<State, Event>(from, e), to);
        }

        public override void Trigger(Event e)
        {
            var key = new KeyValuePair<State, Event>(Current, e);
            if (!_table.ContainsKey(key))
                throw new StateNotFoundException();
            Current = _table[key];
        }
    }

    /// <summary>
    /// Non-deterministic finite-state machine
    /// </summary>
    public class NFA<Event> : Automaton<Event>
    {
        private readonly Dictionary<KeyValuePair<State, Event>, States> _table;
        private readonly Dictionary<State, States> _epsClosures;

        public States Current { get; private set; }

        public NFA()
        {
            _table = new Dictionary<KeyValuePair<State, Event>, States>();
            _epsClosures = new Dictionary<State, States> { { _start, new States { _start } } };
        }

        public override void AddTransition(State from, State to, Event e)
        {
            base.AddTransition(from, to, e);
            var key = new KeyValuePair<State, Event>(from, e);
            if (!_table.ContainsKey(key))
                _table.Add(key, new States());
            _table[key].Add(to);
        }

        /// <summary>
        /// Adds epsilon transition
        /// </summary>
        public void AddTransition(State from, State to)
        {
            if (!_states.Contains(from))
                throw new StateNotFoundException();

            if (_states.Add(to))
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
            Current.UnionWith(_epsClosures[_start]);
        }

        private States TransitState(States st, Event e)
        {
            var state = new States();
            foreach (State s in st)
            {
                States tmp;
                if (_table.TryGetValue(new KeyValuePair<State, Event>(s, e), out tmp))
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

        public override void Trigger(Event e)
        {
            Current = TransitState(Current, e);
            if (Current.Count == 0)
                throw new StateNotFoundException();
        }

        public DFA<Event> ToDFA()
        {
            var a = new DFA<Event>();
            var q = new Queue<States>();
            var table = new Dictionary<States, State>();
            var used = new HashSet<States>();

            q.Enqueue(_epsClosures[_start]);
            table.Add(q.Peek(), a.Start);

            while (q.Count > 0)
            {
                var state = q.Dequeue();
                if (!used.Add(state))
                    continue;

                foreach (Event e in _events)
                {
                    var next = TransitState(state, e);
                    if (next.Count == 0)
                        continue;

                    q.Enqueue(next);

                    State final = next.FirstOrDefault(s => s.Final);
                    State newState = final == null ? new State() : new State(final);
                    table[next] = newState;

                    a.AddTransition(table[state], newState, e);
                }
            }

            return a;
        }

        public void Merge(NFA<Event> other)
        {
            _states.UnionWith(other._states);
            _events.UnionWith(other._events);

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
