using System;
using System.IO;
using System.Text;
using System.Xml;
using StateMachine;

namespace LexicalAnalyzer
{
    public class UnknownTokenException : Exception
    {
        public UnknownTokenException()
        {
        }

        public UnknownTokenException(string message) : base(message)
        {
        }

        public UnknownTokenException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class Lexer : IDisposable
    {
        private XmlReader _reader;
        private Automaton<char> _machine;
        private TextReader _stream;

        public string Token { get; private set; }

        public string TokenType { get; private set; }

        public Lexer()
        {
        }

        public Lexer(string lexisPath)
        {
            LoadLexis(lexisPath);
        }

        public void LoadLexis(string path)
        {
            _reader = XmlReader.Create(path);
            BuildMachine();
        }

        public void SetSource(Stream stream)
        {
            _stream = new StreamReader(stream);
        }

        public void SetSource(string source)
        {
            _stream = new StringReader(source);
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
            Token = token;
            TokenType = _machine.Name;
            _machine.Initial();
        }

        private void BuildMachine()
        {
            var builder = new RegexBuilder();
            while (_reader.Read())
            {
                if (!_reader.IsStartElement()) continue;

                string name, regex;
                switch (_reader.Name)
                {
                    case "lexis":
                        continue;
                    case "token":
                        name = _reader.GetAttribute("name");
                        regex = _reader.GetAttribute("expr");
                        break;
                    default:
                        throw new XmlException($"Lexis file cannot contain '{_reader.Name}' element");
                }

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(regex))
                    throw new XmlException("Token attributes cannot be empty");
                builder.AddExpression(name, regex);
            }
            builder.Build();
            _machine = builder.Machine.ToDFA();
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
