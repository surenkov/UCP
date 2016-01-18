using System;
using System.Collections.Generic;
using System.Text;

namespace LexicalAnalyzer
{
    public class InvalidRegexException : Exception
    {
        public InvalidRegexException()
        {
        }

        public InvalidRegexException(string message) : base(message)
        {
        }

        public InvalidRegexException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

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
            int rpos = 0;
            for (int lpos = 0; (lpos = regex.IndexOf('[', lpos)) != -1; lpos = rpos + 1)
            {
                int rtmp = rpos == 0 ? 0 : rpos + 1;
                tmp.Append(regex.Substring(rtmp, lpos - rtmp));
                rpos = regex.IndexOf(']', lpos);
                if (rpos == -1)
                    throw new InvalidRegexException("Bad brackets sequence");
                int range = lpos + 1;
                tmp.Append('(');
                for (int next; (next = regex.IndexOf('-', range, rpos - range)) != -1; range = next + 1)
                {
                    if (next != range + 1)
                        tmp.Append('|');
                    char from = regex[next - 1];
                    char to = regex[next + 1];
                    if (from > to)
                        throw new InvalidRegexException("Invalid characters range");
                    tmp.Append(from);
                    while (from < to)
                    {
                        tmp.Append('|');
                        tmp.Append(++from);
                    }
                }
                for (int i = range + 1; i < rpos; i++)
                    tmp.Append(regex[i]);
                tmp.Append(')');
            }
            regex = tmp.Append(regex.Substring(rpos == 0 ? 0 : rpos + 1)).ToString();
            tmp.Clear();

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
