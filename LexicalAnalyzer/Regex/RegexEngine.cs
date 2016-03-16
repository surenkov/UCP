using System.Collections.Generic;
using System.IO;
using StateMachine.Automata;
using StateMachine.Stack;
using StateMachine.States;

namespace LexicalAnalyzer.Regex
{
    public class RegexEngine
    {
        private static readonly Dictionary<char, char> EscapeDictionary = new Dictionary<char, char>
        {
            { 'n', '\n' },
            { 'r', '\r' },
            { 't', '\t' },
            { 'v', '\v' },
            { 's', ' ' }
        };

        private static char Escape(char c)
        {
            return EscapeDictionary.ContainsKey(c) ? EscapeDictionary[c] : c;
        }

        private readonly MachineStack<char> _stack;
        public RegexEngine()
        {
            _stack = new MachineStack<char>();
        }

        public void AddExpression(string name, string regex)
        {
            using (var stream = new StringReader(InfixToPostfix.Convert(regex)))
            {
                while (stream.Peek() != -1)
                {
                    var c = (char) stream.Read();
                    switch (c)
                    {
                        case '.': _stack.Concatenate(); break;
                        case '|': _stack.Unite(); break;
                        case '*': _stack.Iterate(); break;
                        case '+': _stack.AtLeast(); break;
                        case '?': _stack.Maybe(); break;
                        default:
                            var a = new NFA<char>();
                            a.AddTransition(a.Start, new State(), c == '\\' ? Escape((char) stream.Read()) : c);
                            _stack.Push(a);
                            break;
                    }
                }

                var top = _stack.Peek();
                top.LastAdded.Final = true;
                top.SetName(top.LastAdded, name);
            }
        }

        public NFA<char> Build()
        {
            while (_stack.Count > 1)
                _stack.Unite();
            return _stack.Peek();
        }
    }
}
