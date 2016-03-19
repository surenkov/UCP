using System;
using System.Collections.Generic;
using StateMachine.States;

namespace StateMachine.Automata
{
    /// <summary>
    ///     Abstract state machine.
    /// </summary>
    [Serializable]
    public abstract class Automaton<TEvent>
        where TEvent : IEquatable<TEvent>
    {
        protected readonly HashSet<TEvent> Events;
        protected readonly Dictionary<ulong, string> Names;
        protected readonly StateSet States;
        protected State StartState;

        protected Automaton()
        {
            States = new StateSet();
            Events = new HashSet<TEvent>();
            Names = new Dictionary<ulong, string>();

            LastAdded = Start = new State();
        }

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

        public virtual void AddTransition(State @from, State to, TEvent e)
        {
            if (!States.Contains(@from))
                throw new StateNotFoundException();

            if (States.Add(to))
                LastAdded = to;
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
}
