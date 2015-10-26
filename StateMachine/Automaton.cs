using System.Linq;
using System.Collections.Generic;

namespace StateMachine
{
    using StateNotFoundException = KeyNotFoundException;
    using States = HashSet<State>;

    public abstract class Automaton<Event>
    {
        private State _start;
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

        public Automaton()
        {
            _states = new HashSet<State>();
            _events = new HashSet<Event>();

            _states.Add(LastAdded = Start = new State());
        }

        public virtual void AddTransition(State from, State to, Event e)
        {
            if (!_states.Contains(from))
                throw new StateNotFoundException();

            if (_states.Add(to)) LastAdded = to;
            _events.Add(e);
        }

        public abstract void Trigger(Event e);
    }

    /// <summary>
    /// Deterministic finite-state machine
    /// </summary>
    public class DFA<Event> : Automaton<Event>
    {
        private Dictionary<KeyValuePair<State, Event>, State> _table;

        public State Current { get; private set; }

        public DFA() : base()
        {
            _table = new Dictionary<KeyValuePair<State, Event>, State>();
            Current = Start;
        }

        public override void AddTransition(State from, State to, Event e)
        {
            base.AddTransition(from, to, e);
            _table.Add(new KeyValuePair<State, Event>(from, e), to);
        }

        public override void Trigger(Event e)
        {
            Current = _table[new KeyValuePair<State, Event>(Current, e)];
        }
    }

    /// <summary>
    /// Non-deterministic finite-state machine
    /// </summary>
    public class NFA<Event> : Automaton<Event>
    {
        private Dictionary<KeyValuePair<State, Event>, States> _table;

        public static Event Epsilon { get; } = default(Event);

        public States Current { get; private set; }

        public NFA() : base()
        {
            _table = new Dictionary<KeyValuePair<State, Event>, States>();
            Current = new States { Start };
            _events.Add(Epsilon);
        }

        public override void AddTransition(State from, State to, Event e)
        {
            base.AddTransition(from, to, e);
            var key = new KeyValuePair<State, Event>(from, e);
            if (!_table.ContainsKey(key))
                _table.Add(key, new States());
            _table[key].Add(to);
        }

        public DFA<Event> toDFA()
        {
            var a = new DFA<Event>();
            var used = new Dictionary<States, State>();
            var q = new Queue<States>();

            var st = transitStates(Epsilon, new States { Start });
            used[st] = a.Start;
            q.Enqueue(st);

            while (q.Count > 0)
            {
                var top = q.Dequeue();
                foreach (var e in _events)
                {
                    q.Enqueue(st = transitStates(e, top));
                    var final = st.FirstOrDefault((s) => s.Final);
                    var newState = new State(final);
                    used[st] = newState;
                    a.AddTransition(used[top], newState, e);
                }
            }
            return a;
        }

        private States transitStates(Event e, States st)
        {
            States tmp = new States(), res = new States();
            foreach (var s in st)
            {
                _table.TryGetValue(new KeyValuePair<State, Event>(s, e), out tmp);
                res.UnionWith(tmp);
            }
            return res;
        }

        public override void Trigger(Event e)
        {
            var state = new States();
            foreach (var s in Current)
            {
                States set;
                _table.TryGetValue(new KeyValuePair<State, Event>(s, e), out set);
                state.UnionWith(set);
                _table.TryGetValue(new KeyValuePair<State, Event>(s, Epsilon), out set);
                state.UnionWith(set);
            }

            if (state.Count == 0)
                throw new StateNotFoundException();

            Current = state;
        }

        public void Merge(NFA<Event> other)
        {
            _states.UnionWith(other._states);
            _events.UnionWith(other._events);
            foreach (var item in other._table)
            {
                if (!_table.ContainsKey(item.Key))
                    _table.Add(item.Key, new States());
                _table[item.Key].UnionWith(item.Value);
            }
        }
    }
}
