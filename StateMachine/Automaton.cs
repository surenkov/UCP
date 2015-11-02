using System;
using System.Linq;
using System.Collections.Generic;

namespace StateMachine
{
    [Serializable]
    public class States : HashSet<State>
    {
        private int _hash;

        public States()
        {
        }

        public States(IEnumerable<State> collection) : base(collection)
        {
        }
        
        public States ReHash()
        {
            unchecked
            {
                int hash = 0;
                foreach (var state in this)
                {
                    hash += state.GetHashCode();
                    hash += (hash << 10);
                    hash ^= (hash >> 6);
                }
                hash += (hash << 3);
                hash ^= (hash >> 11);
                _hash = hash + (hash << 15);
            }
            return this;
        }

        public override bool Equals(object obj)
        {
            var other = obj as States;
            if (other == null || other.Count != Count)
                return false;
            return other.All(Contains);
        }

        public override int GetHashCode() => _hash;
    }

    [Serializable]
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
        protected readonly States States;
        protected readonly HashSet<TEvent> Events;
        protected readonly Dictionary<ulong, string> Names;

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

        public abstract bool IsFinal { get; }

        public abstract string[] Name { get; }

        protected Automaton()
        {
            States = new States();
            Events = new HashSet<TEvent>();
            Names = new Dictionary<ulong, string>();

            States.Add(LastAdded = Start = new State());
        }

        public virtual void AddTransition(State from, State to, TEvent e)
        {
            if (!States.Contains(from))
                throw new StateNotFoundException();

            if (States.Add(to))
            {
                States.ReHash();
                LastAdded = to;
            }
            Events.Add(e);
        }

        public void SetName(State st, string name)
        {
            if (!States.Contains(st))
                throw new StateNotFoundException();
            Names[st.Id] = name;
        }

        public abstract void Trigger(TEvent e);

        public abstract void Initial();
    }

    /// <summary>
    /// Deterministic finite-state machine
    /// </summary>
    public class DFA<TEvent> : Automaton<TEvent>
    {
        private readonly Dictionary<KeyValuePair<State, TEvent>, State> _table;

        public State Current { get; private set; }

        public override string[] Name => Names[Current.Id].Split(';');

        public override bool IsFinal => Current.Final;

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

        public override void Initial()
        {
            Current = Start;
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

        public override string[] Name => Current.Where(s => Names.ContainsKey(s.Id)).Select(s => Names[s.Id]).ToArray(); 
        public override bool IsFinal => Current.Any(s => s.Final);
            
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
            var st = _table[key];
            st.Add(to);
            st.ReHash();
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

        public override void Initial()
        {
            Current = new States();
            Current.UnionWith(_epsClosures[StartState]);
            Current.ReHash();
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

            return epsilonState.ReHash();
        }

        public override void Trigger(TEvent e)
        {
            var state = TransitState(Current, e);
            if (state == null || state.Count == 0)
            {
                Current = new States(Current.Where(s => s.Final && Names.ContainsKey(s.Id)));
                throw new StateNotFoundException();
            }
            Current = state;
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

                foreach (var e in Events)
                {
                    var next = TransitState(state, e);
                    if (next.Count == 0)
                        continue;
                    q.Enqueue(next);

                    var finals = next.Where(s => s.Final).ToArray();
                    var name = string.Join(";", finals
                            .Where(s => Names.ContainsKey(s.Id))
                            .Select(s => Names[s.Id]));
                    var newState = map.ContainsKey(next)
                        ? map[next]
                        : new State
                        {
                            Start = next.FirstOrDefault(s => s.Start) != null,
                            Final = finals.Length > 0,
                        };

                    map[next] = newState;
                    a.AddTransition(map[state], newState, e);
                    if (finals.Length > 0)
                        a.SetName(newState, name);
                }
            }
            return a;
        }

        public void Merge(NFA<TEvent> other)
        {
            States.UnionWith(other.States);
            Events.UnionWith(other.Events);

            foreach (var pair in other.Names)
                Names[pair.Key] = pair.Value;

            foreach (var item in other._epsClosures)
            {
                if (!_epsClosures.ContainsKey(item.Key))
                    _epsClosures.Add(item.Key, new States());
                _epsClosures[item.Key].UnionWith(item.Value);
                _epsClosures[item.Key].ReHash();
            }

            foreach (var item in other._table)
            {
                if (!_table.ContainsKey(item.Key))
                    _table.Add(item.Key, new States());
                _table[item.Key].UnionWith(item.Value);
                _table[item.Key].ReHash();
            }
        }
    }
}
