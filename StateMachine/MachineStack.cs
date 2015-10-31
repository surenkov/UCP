using System.Collections.Generic;

namespace StateMachine
{
    public class MachineStack<TEvent>
    {
        private readonly Stack<NFA<TEvent>> _stack;

        public MachineStack()
        {
            _stack = new Stack<NFA<TEvent>>();
        }

        public void Push(NFA<TEvent> a)
        {
            _stack.Push(a);
        }

        public NFA<TEvent> Pop()
        {
            return _stack.Pop();
        }

        public NFA<TEvent> Peek()
        {
            return _stack.Peek();
        }

        public int Count => _stack.Count;

        /// <summary>
        /// Sequental concatenation
        /// </summary>
        public void Concatenate()
        {
            if (_stack.Count < 2)
                return;

            var a1 = _stack.Pop();
            var a2 = _stack.Pop();

            var res = new NFA<TEvent>();
            res.Merge(a1);
            res.Merge(a2);

            a1.Start.Start = a2.Start.Start = false;
            a1.LastAdded.Final = a2.LastAdded.Final = false;
            res.AddTransition(res.Start, a2.Start);
            res.AddTransition(a2.LastAdded, a1.Start);
            res.AddTransition(a1.LastAdded, new State { Final = true });

            _stack.Push(res);
        }

        /// <summary>
        /// Parallel concatenation
        /// </summary>
        public void Unite()
        {
            if (_stack.Count < 2)
                return;

            var a1 = _stack.Pop();
            var a2 = _stack.Pop();

            var res = new NFA<TEvent>();
            res.Merge(a1);
            res.Merge(a2);

            a1.Start.Start = a2.Start.Start = false;
            a1.LastAdded.Final = a2.LastAdded.Final = false;
            res.AddTransition(res.Start, a2.Start);
            res.AddTransition(res.Start, a1.Start);
            res.AddTransition(a2.LastAdded, new State { Final = true });
            res.AddTransition(a1.LastAdded, res.LastAdded);

            _stack.Push(res);
        }

        /// <summary>
        /// Iteration (Kleene *)
        /// </summary>
        public void Iterate()
        {
            if (_stack.Count < 1)
                return;

            var a = _stack.Pop();
            var res = new NFA<TEvent>();
            res.Merge(a);

            a.Start.Start = a.LastAdded.Final = false;
            res.AddTransition(res.Start, new State { Final = true });
            res.AddTransition(res.LastAdded, res.Start);
            res.AddTransition(res.Start, a.Start);
            res.AddTransition(a.LastAdded, res.LastAdded);

            _stack.Push(res);
        }

        /// <summary>
        /// Kleene ?
        /// </summary>
        public void Maybe()
        {
            if (_stack.Count < 1)
                return;

            var a = _stack.Pop();
            var res = new NFA<TEvent>();
            res.Merge(a);

            a.Start.Start = a.LastAdded.Final = false;
            res.AddTransition(res.Start, new State { Final = true });
            res.AddTransition(res.Start, a.Start);
            res.AddTransition(a.LastAdded, res.LastAdded);

            _stack.Push(res);
        }

        /// <summary>
        /// Kleene +
        /// </summary>
        public void AtLeast()
        {
            if (_stack.Count < 1)
                return;

            var a = _stack.Pop();
            var res = new NFA<TEvent>();
            res.Merge(a);

            a.Start.Start = a.LastAdded.Final = false;
            res.AddTransition(res.Start, a.Start);
            res.AddTransition(a.LastAdded, new State { Final = true });
            res.AddTransition(res.LastAdded, res.Start);

            _stack.Push(res);
        }
    }
}
