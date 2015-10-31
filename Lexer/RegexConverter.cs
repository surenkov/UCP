using System.Collections.Generic;
using System.Text;

namespace Lexer
{
    public static class RegexConverter
    {
        private static readonly Dictionary<char, int> Precedence;
        private static readonly HashSet<char> NotAfter;
        private static readonly HashSet<char> NotBefore;

        static RegexConverter()
        {
            Precedence = new Dictionary<char, int> {
                { '(', 1 }, { '|', 2 }, { '.', 3 }, { '?', 4 }, { '*', 4 }, { '+', 4 }
            };
            NotAfter = new HashSet<char> { '(', '|', '.' };
            NotBefore = new HashSet<char> { ')', '|', '.', '?', '*', '+' };
        }

        private static int Prec(char c)
        {
            return Precedence.ContainsKey(c) ? Precedence[c] : 6;
        }

        public static string Postfix(string regex)
        {
            var tmp = new StringBuilder(regex.Length * 2);
            for (int i = 1; i < regex.Length; i++)
            {
                tmp.Append(regex[i - 1]);
                if (NotAfter.Contains(regex[i - 1]) || NotBefore.Contains(regex[i]))
                    continue;
                tmp.Append('.');
            }
            if (regex.Length > 0)
                tmp.Append(regex[regex.Length - 1]);

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
                        while (stack.Count > 0 && Prec(stack.Peek()) >= Prec(c))
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
