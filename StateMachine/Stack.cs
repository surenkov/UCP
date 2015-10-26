using System;
using System.Collections.Generic;

namespace StateMachine
{
    public class MachineStack<Event>
    {
        private Stack<NFA<Event>> _stack;

        public MachineStack()
        {
            _stack = new Stack<NFA<Event>>();
        }

        public void Push(NFA<Event> a)
        {
            _stack.Push(a);
        }

        public NFA<Event> Pop()
        {
            return _stack.Pop();
        }

        public NFA<Event> Peek()
        {
            return _stack.Peek();
        }

        /// <summary>
        /// Sequental concatenation
        /// </summary>
        public void Concatenate()
        {
            var a1 = _stack.Pop();
            var a2 = _stack.Pop();

            var res = new NFA<Event>();
            res.Merge(a1);
            res.Merge(a2);
            var eps = NFA<Event>.Epsilon;

            res.AddTransition(res.Start, a1.Start, eps);
            res.AddTransition(a1.LastAdded, a2.Start, eps);
            res.AddTransition(a2.LastAdded, new State(), eps);
            res.LastAdded.Final = true;

            _stack.Push(res);
        }

        /// <summary>
        /// Parallel concatenation
        /// </summary>
        public void Unite()
        {
            var a1 = _stack.Pop();
            var a2 = _stack.Pop();

            var res = new NFA<Event>();
            res.Merge(a1);
            res.Merge(a2);
            var eps = NFA<Event>.Epsilon;

            res.AddTransition(res.Start, a1.Start, eps);
            res.AddTransition(res.Start, a2.Start, eps);
            res.AddTransition(a1.LastAdded, new State(), eps);
            res.AddTransition(a2.LastAdded, res.LastAdded, eps);
            res.LastAdded.Final = true;

            _stack.Push(res);
        }

        /// <summary>
        /// Iteration (Kleene *)
        /// </summary>
        public void Iterate()
        {
            var a = _stack.Pop();
            var eps = NFA<Event>.Epsilon;
            var res = new NFA<Event>();
            res.Merge(a);

            res.AddTransition(res.Start, new State(), eps);
            res.AddTransition(res.LastAdded, res.Start, eps);
            res.AddTransition(res.Start, a.Start, eps);
            res.AddTransition(a.LastAdded, res.LastAdded, eps);
            res.LastAdded.Final = true;

            _stack.Push(res);
        }

        /// <summary>
        /// Kleene ?
        /// </summary>
        public void Maybe()
        {
            var a = _stack.Pop();
            var eps = NFA<Event>.Epsilon;
            var res = new NFA<Event>();
            res.Merge(a);

            res.AddTransition(res.Start, new State(), eps);
            res.AddTransition(res.Start, a.Start, eps);
            res.AddTransition(a.LastAdded, res.LastAdded, eps);
            res.LastAdded.Final = true;

            _stack.Push(res);
        }

        /// <summary>
        /// Kleene +
        /// </summary>
        public void AtLeast()
        {
            var a = _stack.Pop();
            var eps = NFA<Event>.Epsilon;
            var res = new NFA<Event>();
            res.Merge(a);

            res.AddTransition(res.Start, a.Start, eps);
            res.AddTransition(a.LastAdded, new State(), eps);
            res.AddTransition(res.LastAdded, res.Start, eps);
            res.LastAdded.Final = true;

            _stack.Push(res);
        }
    }
}
