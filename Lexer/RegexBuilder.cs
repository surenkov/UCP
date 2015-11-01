using StateMachine;

namespace LexicalAnalyzer
{
    public class RegexBuilder
    {
        private readonly MachineStack<char> _stack;

        public NFA<char> Machine => _stack.Peek();

        public RegexBuilder()
        {
            _stack = new MachineStack<char>();
        }

        public void AddExpression(string name, string regex)
        {
            foreach (char c in RegexConverter.Postfix(regex))
            {
                switch (c)
                {
                    case '.':
                        _stack.Concatenate();
                        break;
                    case '|':
                        _stack.Unite();
                        break;
                    case '*':
                        _stack.Iterate();
                        break;
                    case '+':
                        _stack.AtLeast();
                        break;
                    case '?':
                        _stack.Maybe();
                        break;
                    default:
                        var a = new NFA<char>();
                        a.AddTransition(a.Start, new State(), c);
                        _stack.Push(a);
                        break;
                }
            }
            var automaton = _stack.Peek();
            automaton.LastAdded.Final = true;
            automaton.SetName(automaton.LastAdded, name);
        }

        public void Build()
        {
            while (_stack.Count > 1)
                _stack.Unite();
        }
    }
}
