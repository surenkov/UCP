using System;
using System.Collections.Generic;

namespace StateMachine.Automata
{
    /// <summary>
    /// Abstract state machine
    /// </summary>
    public abstract class Automaton<TEvent>
        where TEvent : IEquatable<TEvent>
    {
        protected State StartState;
        protected readonly States States;
        protected readonly HashSet<TEvent> Events;
        protected readonly Dictionary<ulong, string> Names;

        public State Start
        {
            get { return StartState; }
            private set
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