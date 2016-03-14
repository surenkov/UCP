using System;
using System.Collections.Generic;
using StateMachine.Utility;

namespace StateMachine.Automata
{
    /// <summary>
    /// Deterministic finite-state machine
    /// </summary>
    public class DFA<TEvent> : Automaton<TEvent>
        where TEvent : IEquatable<TEvent>
    {
        private readonly Dictionary<KeyValuePair<State, TEvent>, State> _table;

        public State Current { get; private set; }

        public override string[] Name => Names[Current.Id].Split(';');

        public override bool IsFinal => Current.Final;

        public DFA()
        {
            _table = new Dictionary<KeyValuePair<State, TEvent>, State>(new StateEventPairEqualityComparer<TEvent>());
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
}