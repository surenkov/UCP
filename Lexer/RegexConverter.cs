using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Lexer
{
    public class RegexConverter
    {
        private static readonly Dictionary<char, int> _precedence;

        static RegexConverter()
        {
            _precedence = new Dictionary<char, int>
            {
                { '(', 1 }, { '|', 2 }, { '.', 3 }, { '?', 4 }, { '*', 4 }, { '+', 4 }
            };
        }


        public string Regex { get; set; }

        private static int Precedence(char c)
        {
            if (_precedence.ContainsKey(c))
                return _precedence[c];
            return 6;
        }

        public string Postfix()
        {
            var stack = new Stack<char>(Regex.Length);
            var postfix = new StringBuilder(Regex.Length);

            foreach (char c in Regex)
            {
                switch (c)
                {
                    case '(':
                        stack.Push(c);
                        break;
                    case ')':
                        while (stack.Peek() != '(')
                            postfix.Append(stack.Pop());
                        stack.Pop();
                        break;
                    default:
                        while (stack.Count > 0 && Precedence(stack.Peek()) >= Precedence(c))
                            postfix.Append(stack.Pop());
                        stack.Push(c);
                        break;
                }
            }

            while (stack.Count > 0)
                postfix.Append(stack.Pop());

            return postfix.ToString();
        }
    }
}
