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

        private string _regex;
        public string Regex
        {
            get { return _regex; }
            set
            {
                if (_regex != value)
                {
                    _regex = value;
                    _postfix = string.Empty;
                }
            }
        }

        private string _postfix;
        public string Postfix
        {
            get
            {
                if (string.IsNullOrEmpty(_postfix))
                    _postfix = ToPostfix();
                return _postfix;
            }
        }

        private static int Precedence(char c)
        {
            if (_precedence.ContainsKey(c))
                return _precedence[c];
            return 6;
        }

        private string ToPostfix()
        {
            var stack = new Stack<char>(_regex.Length);
            var postfix = new StringBuilder(_regex.Length);

            foreach (char c in _regex)
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
