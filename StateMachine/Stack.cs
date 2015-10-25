using System;
using System.Collections.Generic;

namespace StateMachine
{
    public class MachineStack<Event>
    {
        private Stack<Automaton<Event>> _stack;

        public MachineStack()
        {
            _stack = new Stack<Automaton<Event>>();
        }

        public void Push(Automaton<Event> a)
        {
            _stack.Push(a);
        }

        public Automaton<Event> Pop()
        {
            return _stack.Pop();
        }

        public void Concatenate()
        {
            throw new NotImplementedException();
        }

        public void Unite()
        {
            throw new NotImplementedException();
        }

        public void Iterate()
        {
            throw new NotImplementedException();
        }

        public void Maybe()
        {
            throw new NotImplementedException();
        }

        public void AtLeast()
        {
            throw new NotImplementedException();
        }
    }
}
