using System;
using System.Collections.Generic;
using System.Linq;
using StateMachine.Utility;

namespace StateMachine.Automata
{
    /// <summary>
    /// Non-deterministic finite-state machine
    /// </summary>
    public class NFA<TEvent> : Automaton<TEvent>
        where TEvent : IEquatable<TEvent>
    {
        private readonly Dictionary<KeyValuePair<State, TEvent>, States> _table;
        private readonly Dictionary<State, States> _epsClosures;

        public States Current { get; private set; }

        public override string[] Name => Current.Where(s => Names.ContainsKey(s.Id)).Select(s => Names[s.Id]).ToArray(); 
        public override bool IsFinal => Current.Any(s => s.Final);
            
        public NFA()
        {
            _table = new Dictionary<KeyValuePair<State, TEvent>, States>(new StateEventPairEqualityComparer<TEvent>());
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

        public override void Initial()
        {
            Current = new States();
            Current.UnionWith(_epsClosures[StartState]);
            Current.ReHash();
        }

        private States TransitState(States st, TEvent e)
        {
            var state = new States();
            foreach (var s in st)
            {
                States tmp;
                if (_table.TryGetValue(new KeyValuePair<State, TEvent>(s, e), out tmp))
                    state.UnionWith(tmp);
            }

            var epsilonState = new States();
            foreach (var s in state)
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
                throw new StateNotFoundException();
            Current = state;
        }

        public DFA<TEvent> ToDFA()
        {
            var a = new DFA<TEvent>();
            var q = new Queue<States>();
            var map = new Dictionary<States, State>();
            var used = new HashSet<States>();

            q.Enqueue(_epsClosures[StartState].ReHash());
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
