using System.Collections.Generic;
using System.Text;

namespace Lexer
{
    public class RegexConverter
    {
        private static readonly Dictionary<char, int> _precedence;
        private static readonly HashSet<char> _notafter;
        private static readonly HashSet<char> _notbefore;

        static RegexConverter()
        {
            _precedence = new Dictionary<char, int> {
                { '(', 1 }, { '|', 2 }, { '.', 3 }, { '?', 4 }, { '*', 4 }, { '+', 4 }
            };
            _notafter = new HashSet<char> { '(', '|', '.' };
            _notbefore = new HashSet<char> { ')', '|', '.', '?', '*', '+' };
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
            return _precedence.ContainsKey(c) ? _precedence[c] : 6;
        }

        private string ToPostfix()
        {
            var tmp = new StringBuilder(_regex.Length * 2);
            for (int i = 1; i < _regex.Length; i++)
            {
                tmp.Append(_regex[i - 1]);
                if (!(_notafter.Contains(_regex[i - 1]) || _notbefore.Contains(_regex[i])))
                    tmp.Append('.');
            }
            if (_regex.Length > 0)
                tmp.Append(_regex[_regex.Length - 1]);

            var stack = new Stack<char>();
            var postfix = new StringBuilder(tmp.Length);

            foreach (char c in tmp.ToString())
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
