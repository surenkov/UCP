using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using StateMachine;

namespace LexicalAnalyzer
{
    public class UnknownTokenException : Exception
    {
        public UnknownTokenException(string message) : base(message)
        {
        }

        public UnknownTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class Token
    {
        public uint Line { get; set; }

        public uint Column { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return $"[Ln {Line}, Col {Column}] <{Type}>: <{Value}>";
        }
    }

    public class Lexer : IDisposable
    {
        private Automaton<char> _machine;
        private readonly Dictionary<string, int> _prec;
        private TextReader _stream;
        private uint _line;
        private uint _column;

        public Token Token { get; private set; }

        public Lexer()
        {
            _prec = new Dictionary<string, int>();
            _line = _column = 0;
        }

        public Lexer(string lexisPath) : this()
        {
            LoadLexis(lexisPath);
        }

        public void LoadLexis(string path)
        {
            var lexis = XmlReader.Create(path);
            var builder = new RegexBuilder();
            while (lexis.Read())
            {
                if (!lexis.IsStartElement()) continue;

                string name, regex, precedence;
                switch (lexis.Name)
                {
                    case "lexis":
                        continue;
                    case "token":
                        name = lexis.GetAttribute("name");
                        regex = lexis.GetAttribute("expression");
                        precedence = lexis.GetAttribute("precedence");
                        break;
                    default:
                        throw new XmlException($"Lexis file cannot contain '{lexis.Name}' element");
                }

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(regex))
                    throw new XmlException("Token attributes cannot be empty");
                builder.AddExpression(name, regex);

                int prec;
                if (!int.TryParse(precedence, out prec))
                    prec = 0;
                _prec[name] = prec;
            }
            builder.Build();
            _machine = builder.Machine;
            _machine.Initial();
        }

        public void SetSource(Stream stream)
        {
            SetSource(new StreamReader(stream));
        }

        public void SetSource(string source)
        {
            SetSource(new StringReader(source));
        }

        private void SetSource(TextReader source)
        {
            _stream = source;
            _line = _column = 1;
        }

        public bool GetToken()
        {
            if (_stream.Peek() == -1)
                return false;

            var token = new StringBuilder();
            try
            {
                while (_stream.Peek() != -1)
                {
                    char c = (char)_stream.Peek();
                    _machine.Trigger(c);
                    token.Append(c);
                    _stream.Read();
                    _column++;
                    if (c != '\n') continue;
                    _line++;
                    _column = 1;
                }
            }
            catch (StateNotFoundException e)
            {
                if (!_machine.IsFinal)
                    throw new UnknownTokenException($"Unknown token: {token}", e);

                SetToken(token.ToString());
                return _stream.Peek() != -1;
            }

            if (!_machine.IsFinal)
                throw new UnknownTokenException("Unexpected end of source");

            SetToken(token.ToString());
            return true;
        }

        private void SetToken(string token)
        {
            var names = _machine.Name;
            Token = new Token
            {
                Column = _column,
                Line = _line,
                Value = token,
                Type = names.Aggregate(names.First(), (cur, str) => _prec[str] > _prec[cur] ? str : cur)
            };
            _machine.Initial();
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
