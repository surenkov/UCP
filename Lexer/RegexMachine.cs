using StateMachine;

namespace Lexer
{
    public class RegexMachine
    {
        private readonly RegexConverter _converter;

        private readonly MachineStack<char> _stack;

        public NFA<char> Machine
        {
            get { return _stack.Peek(); }
        }

        public RegexMachine()
        {
            _converter = new RegexConverter();
            _stack = new MachineStack<char>();
        }

        public void AddExpression(string name, string regex)
        {
            _converter.Regex = regex;
            foreach (char c in _converter.Postfix)
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
                        a.AddTransition(a.Start, new State { Final = true, Name = c.ToString() }, c);
                        _stack.Push(a);
                        break;
                }
            }
            var automaton = _stack.Peek();
            automaton.LastAdded.Name = name;
            automaton.Initialize();
        }
    }
}
