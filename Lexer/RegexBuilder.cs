using System.Collections.Generic;
using System.IO;
using StateMachine;

namespace LexicalAnalyzer
{
    public class RegexBuilder
    {
        private readonly MachineStack<char> _stack;
        private static readonly Dictionary<char, char> EscapeDictionary;

        public NFA<char> Machine => _stack.Peek();

        static RegexBuilder()
        {
            EscapeDictionary = new Dictionary<char, char>
            {
                { 'n', '\n' },
                { '\\', '\\' },
                { 't', '\t' },
                { 's', ' ' }
            };
        }

        private static char Escape(char c)
        {
            return EscapeDictionary.ContainsKey(c) ? EscapeDictionary[c] : c;
        }

        public RegexBuilder()
        {
            _stack = new MachineStack<char>();
        }

        public void AddExpression(string name, string regex)
        {
            var stream = new StringReader(InfixToPostfix.Convert(regex));
            while (stream.Peek() != -1)
            {
                var c = (char) stream.Read();
                NFA<char> a;
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
                    case '\\':
                        char n = (char) stream.Read();
                        a = new NFA<char>();
                        a.AddTransition(a.Start, new State(), Escape(n));
                        _stack.Push(a);
                        break;
                    default:
                        a = new NFA<char>();
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
