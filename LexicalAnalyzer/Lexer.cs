﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using LexicalAnalyzer.Regex;
using StateMachine.Automata;
using StateMachine.States;

namespace LexicalAnalyzer
{
    /// <summary>
    /// Interface for tokens' recognizer automaton.
    /// </summary>
    [Serializable]
    public class Lexer : IDisposable, IEnumerable<Token>
    {
        private Automaton<char> _machine;
        private readonly Dictionary<string, int> _prec;
        private readonly Dictionary<string, bool> _required;
        [NonSerialized] private TextReader _stream;
        [NonSerialized] private uint _line;
        [NonSerialized] private uint _column;

        public Token Token { get; private set; }

        /// <summary>
        /// <para>Sets lexer to produce null-token after the end of source.</para>
        /// <remarks>Useful for some parsing algorithms</remarks>
        /// </summary>
        public bool YieldEndOfSource { get; set; }

        public Lexer()
        {
            _prec = new Dictionary<string, int>();
            _required = new Dictionary<string, bool>();
            _line = _column = 0;
            YieldEndOfSource = false;
        }

        public Lexer(string lexisPath, bool dfa = false) : this()
        {
            LoadLexis(lexisPath, dfa);
        }

        public Lexer(Stream stream, bool dfa = false) : this()
        {
            LoadLexis(stream, dfa);
        }

        /// <summary>
        /// Loads lexis from specified file and builds recognizing automaton
        /// </summary>
        /// <param name="path">Lexis specification file path</param>
        /// <param name="deterministic">See description of the same parameter of <see cref="LoadLexis(Stream, bool)"/></param>
        public void LoadLexis(string path, bool deterministic = false)
        {
            LoadLexis(new FileStream(path, FileMode.Open), deterministic);
        }

        /// <summary>
        /// Loads lexis from stream and builds recognizing automaton
        /// </summary>
        /// <param name="stream">
        ///     <para>Lexis specification stream</para>
        ///     <remarks>See schema at Schemas\LexisSchema.xsd</remarks>
        /// </param>
        /// <param name="deterministic">
        ///     <para>Flag, which specifies automaton type:
        /// 	deterministic or non-deterministic.</para>
        /// 	<remarks>DFA's building is significantly slower (creates up to 2^n of NFA states),
        /// 	but recognizes much faster (O(n) instead of O(n*m), where n - token's length)</remarks>
        /// </param>
        public void LoadLexis(Stream stream, bool deterministic = false)
        {
            var schemas = new XmlSchemaSet();
            string pwd = AppDomain.CurrentDomain.BaseDirectory;
            schemas.Add("http://savva.moe/compiler/lexis.xsd", Path.Combine(pwd, "Schemas", "LexisSchema.xsd"));
            var lexis = XmlReader.Create(stream, new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = schemas
            });
            var builder = new RegexEngine();
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
                        throw new XmlException($"Lexis definition cannot contain '{lexis.Name}' element");
                }

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(regex))
                    throw new XmlException("Token attributes cannot be empty");
                builder.AddExpression(name, regex);

                int prec;
                int.TryParse(precedence, out prec);
                _prec[name] = prec;

                bool omit;
                bool.TryParse(canBeOmitted, out omit);
                _required[name] = !omit;
            }
            _machine = deterministic ? (Automaton<char>) builder.Build().ToDFA() : builder.Build();
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
            uint column = _column;
            try
            {
                while (_stream.Peek() != -1)
                {
                    char c = (char) _stream.Peek();
                    _machine.Trigger(c);
                    token.Append(c);
                    _stream.Read();
                    column++;
                    if (c != '\n') continue;
                    _line++;
                    column = 1;
                }
            }
            catch (StateNotFoundException e)
            {
                if (!_machine.IsFinal)
                    UnknownToken($"Unknown token: <{token}>, Ln: {_line} Col: {_column}", e);

                SetToken(token.ToString());
                _column = column;
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
            Token.Required = _required[Token.Type];
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
            if (YieldEndOfSource)
                yield return null;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}