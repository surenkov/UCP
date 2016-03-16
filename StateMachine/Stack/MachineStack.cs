using System;
using System.Collections;
using System.Collections.Generic;
using StateMachine.Automata;
using StateMachine.States;

namespace StateMachine.Stack
{
    public class MachineStack<TEvent> : IEnumerable<NFA<TEvent>>
        where TEvent : IEquatable<TEvent>
    {
        private readonly Stack<NFA<TEvent>> _stack;

        public MachineStack()
        {
            _stack = new Stack<NFA<TEvent>>();
        }

        /// <summary>
        ///     Gets the number of automata contained in <see cref="MachineStack{TEvent}" />
        /// </summary>
        public int Count => _stack.Count;

        public IEnumerator<NFA<TEvent>> GetEnumerator() => _stack.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        ///     Removes all automata from the <see cref="MachineStack{TEvent}" />.
        /// </summary>
        public void Clear() => _stack.Clear();

        /// <summary>
        ///     Inserts an <see cref="automaton" /> on top of <see cref="MachineStack{TEvent}" />.
        /// </summary>
        public void Push(NFA<TEvent> automaton) => _stack.Push(automaton);

        /// <summary>
        ///     Removes and returns the automaton at the top of <see cref="MachineStack{TEvent}" />.
        /// </summary>
        /// <returns>Removed top of stack.</returns>
        public NFA<TEvent> Pop() => _stack.Pop();

        /// <summary>
        ///     Returns the automaton at the top of the <see cref="MachineStack{TEvent}" />
        ///     without removing it.
        /// </summary>
        public NFA<TEvent> Peek() => _stack.Peek();

        /// <summary>
        ///     Sequental concatenation.
        /// </summary>
        public void Concatenate()
        {
            ThrowIfStackIsEmpty(2);

            var a1 = _stack.Pop();
            var a2 = _stack.Peek();
            a2.Merge(a1);

            a2.AddTransition(a2.LastAdded, a1.Start);
            a2.AddTransition(a1.LastAdded, new State { Final = true });
        }

        /// <summary>
        ///     Parallel unification.
        /// </summary>
        public void Unite()
        {
            ThrowIfStackIsEmpty(2);

            var a1 = _stack.Pop();
            var a2 = _stack.Peek();
            a2.Merge(a1);

            var start = a2.Start;
            a2.Start = new State();

            a2.AddTransition(a2.Start, start);
            a2.AddTransition(a2.LastAdded, new State { Final = true });
            a2.AddTransition(a2.Start, a1.Start);
            a2.AddTransition(a1.LastAdded, a2.LastAdded);
        }

        /// <summary>
        ///     Iteration (Kleene *).
        /// </summary>
        public void Iterate()
        {
            ThrowIfStackIsEmpty();

            var a = _stack.Peek();
            var start = a.Start;
            var last = a.LastAdded;
            a.Start = new State();

            a.AddTransition(a.Start, new State { Final = true });
            a.AddTransition(a.LastAdded, a.Start);
            a.AddTransition(a.Start, start);
            a.AddTransition(last, a.LastAdded);
        }

        /// <summary>
        ///     Kleene +.
        /// </summary>
        public void AtLeast()
        {
            ThrowIfStackIsEmpty();

            var a = _stack.Peek();
            var start = a.Start;
            a.Start = new State();

            a.AddTransition(a.Start, start);
            a.AddTransition(a.LastAdded, new State { Final = true });
            a.AddTransition(a.LastAdded, a.Start);
        }

        /// <summary>
        ///     Kleene ?.
        /// </summary>
        public void Maybe()
        {
            ThrowIfStackIsEmpty();

            var a = _stack.Peek();
            var start = a.Start;
            a.Start = new State();

            a.AddTransition(a.LastAdded, new State { Final = true });
            a.AddTransition(a.Start, a.LastAdded);
            a.AddTransition(a.Start, start);
        }

        /// <summary>
        ///     Throws <see cref="IndexOutOfRangeException" /> if
        ///     <see cref="Count" /> is less than
        ///     <see cref="count" />
        /// </summary>
        /// <param name="count">Minimal count of stack' items</param>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="count" /> is zero-negative</exception>
        /// <exception cref="IndexOutOfRangeException"><see cref="Count" /> is less than <see cref="count" /></exception>
        private void ThrowIfStackIsEmpty(int count = 1)
        {
            if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (_stack.Count < count) throw new IndexOutOfRangeException("Automata stack is empty");
        }
    }
}
