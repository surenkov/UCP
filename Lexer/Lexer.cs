using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
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

        public bool CanBeOmitted { get; set; }

        public override string ToString()
        {
            return $"[Ln {Line}, Col {Column}] <{Type}>: <{Value}>";
        }
    }

    [Serializable]
    public class Lexer : IDisposable, IEnumerable<Token>
    {
        private Automaton<char> _machine;
        private readonly Dictionary<string, int> _prec;
        private readonly Dictionary<string, bool> _omit;
        [NonSerialized] private TextReader _stream;
        [NonSerialized] private uint _line;
        [NonSerialized] private uint _column;

        public Token Token { get; private set; }

        public Lexer()
        {
            _prec = new Dictionary<string, int>();
            _omit = new Dictionary<string, bool>();
            _line = _column = 0;
        }

        public Lexer(string lexisPath, bool dfa = false) : this()
        {
            LoadLexis(lexisPath, dfa);
        }

        public Lexer(Stream stream, bool dfa = false) : this()
        {
            LoadLexis(stream, dfa);
        }

        public void LoadLexis(string path, bool dfa)
        {
            LoadLexis(new FileStream(path, FileMode.Open), dfa);
        }

        public void LoadLexis(Stream stream, bool dfa)
        {
            var schemas = new XmlSchemaSet();
            schemas.Add("http://savva.moe/compiler/lexis.xsd", "Schemas/LexisSchema.xsd");
            var lexis = XmlReader.Create(stream, new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = schemas
            });
            var builder = new RegexBuilder();
            while (lexis.Read())
            {
                if (!lexis.IsStartElement()) continue;

                string name, regex, precedence, canBeOmitted;
                switch (lexis.Name)
                {
                    case "lexis":
                        continue;
                    case "token":
                        name = lexis.GetAttribute("name");
                        regex = lexis.GetAttribute("expression");
                        precedence = lexis.GetAttribute("precedence");
                        canBeOmitted = lexis.GetAttribute("omit");
                        break;
                    default:
                        throw new XmlException($"Lexis file cannot contain '{lexis.Name}' element");
                }

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(regex))
                    throw new XmlException("Token attributes cannot be empty");
                builder.AddExpression(name, regex);

                int prec;
                int.TryParse(precedence, out prec);
                _prec[name] = prec;

                bool omit;
                bool.TryParse(canBeOmitted, out omit);
                _omit[name] = omit;
            }
            builder.Build();
            if (dfa)
                _machine = builder.Machine.ToDFA();
            else
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
                    UnknownToken($"Unknown token: <{token}>, Ln: {_line} Col: {_column}", e);

                SetToken(token.ToString());
                return _stream.Peek() != -1;
            }

            if (!_machine.IsFinal)
                UnknownToken("Unexpected end of source");

            SetToken(token.ToString());
            return true;
        }

        private void UnknownToken(string message, Exception innerException = null)
        {
            _machine.Initial();
            throw new UnknownTokenException(message, innerException);
        }

        private void SetToken(string token)
        {
            var names = _machine.Name;
            Token = new Token
            {
                Column = _column,
                Line = _line,
                Value = token,
                Type = names.Aggregate(names.First(), (cur, str) => _prec[str] > _prec[cur] ? str : cur),
            };
            Token.CanBeOmitted = _omit[Token.Type];
            _machine.Initial();
        }

        public void Dispose()
        {
            _stream.Dispose();
        }

        public IEnumerator<Token> GetEnumerator()
        {
            while (GetToken())
                yield return Token;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
