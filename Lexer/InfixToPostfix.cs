using System; 
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LexicalAnalyzer
{
    public class InvalidRegexException : Exception
    {
        public InvalidRegexException(string message) : base(message)
        {
        }
    }

    public class InfixToPostfix
    {
        private static readonly Dictionary<char, int> Precedence;
        private static readonly Dictionary<char, string> Escaped;
        private static readonly HashSet<char> NotAfter;
        private static readonly HashSet<char> NotBefore;
        private static readonly HashSet<char> ToEscape;

        static InfixToPostfix()
        {
            Precedence = new Dictionary<char, int> {
                { '(', 1 }, { '|', 2 }, { '.', 3 }, { '?', 4 }, { '*', 4 }, { '+', 4 }, { '\\', 6 }
            };
            NotAfter = new HashSet<char> { '(', '|', '.', '\\' };
            NotBefore = new HashSet<char> { ')', '|', '.', '?', '*', '+' };
            ToEscape = new HashSet<char> { '(', ')', '[', ']', '{', '}', '|', '.', '+', '?', '*', '-' };
            Escaped = new Dictionary<char, string> { { 'n', "\\\n" }, { '0', "\\\0" }, { 't', "\\\t" } };
        }

        public static string Convert(string regex)
        {
            return new InfixToPostfix(regex).Parse();
        }

        private static int Prec(char c)
        {
            return Precedence.ContainsKey(c) ? Precedence[c] : 6;
        }

        private static string Escape(char c)
        {
            return Escaped.ContainsKey(c) ? Escaped[c] : "\\" + c;
        }

        private static void Error(string message)
        {
            throw new InvalidRegexException(message);
        }


        private string _regex;

        private InfixToPostfix(string regex)
        {
            _regex = regex;
            Prepare();
        }

        private void Prepare()
        {
            if (string.IsNullOrEmpty(_regex)) Error("Regex cannot be empty");
            var reader = new StringReader(_regex);
            var tmp = new StringBuilder(_regex.Length);

            bool range = false;
            while (reader.Peek() != -1)
            {
                char c = (char) reader.Read();
                int n = reader.Peek();
                bool escape = c == '\\' && ToEscape.Contains((char) n);
                if (escape) c = (char) reader.Read();

                bool start = !escape && c == '[';
                bool finish = !escape && c == ']';
                if (start) { range = true; tmp.Append('('); }
                else if (finish) { range = false; tmp.Remove(tmp.Length - 1, 1).Append(')'); }
                else if (!range) { tmp.Append(escape ? Escape(c) : c.ToString()); }
                else
                {
                    if (n == '-' && !escape)
                    {
                        reader.Read();
                        n = (char) reader.Peek();
                        for (char i = c; i < n; i++)
                            tmp.Append(i).Append('|');
                    }
                    else tmp.Append(escape ? Escape(c) : c.ToString()).Append('|');
                }
            }

            reader = new StringReader(tmp.ToString());
            tmp = new StringBuilder(tmp.Length * 2);
            bool printDot = false;
            while (reader.Peek() != -1)
            {
                char c = (char) reader.Read();
                int n = reader.Peek();
                bool escape = c == '\\' && ToEscape.Contains((char) n);
                if (escape) c = (char) reader.Read();

                if (printDot && !NotBefore.Contains(c) || printDot && escape)
                    tmp.Append('.');

                tmp.Append(escape ? Escape(c) : c.ToString());
                printDot = !NotAfter.Contains(c) || escape;
            }

            _regex = tmp.ToString();
        }

        private string Parse()
        {
            var stack = new Stack<char>();
            var postfix = new StringBuilder(_regex.Length);
            var reader = new StringReader(_regex);

            while (reader.Peek() != -1)
            {
                char c = (char) reader.Read();
                switch (c)
                {
                    case '\\':
                        while (stack.Count > 0 && Prec(stack.Peek()) >= Prec(c))
                            postfix.Append(stack.Pop());
                        postfix.Append("\\" + (char)reader.Read());
                        break;
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
