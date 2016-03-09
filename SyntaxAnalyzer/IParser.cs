using System;
using System.Collections.Generic;
using LexicalAnalyzer;
using SyntaxAnalyzer.AST;

namespace SyntaxAnalyzer
{
    public class SyntaxException : Exception
    {
        public SyntaxException()
        {
        }

        public SyntaxException(string message)
            : base(message)
        {
        }
    }

    public interface IParser
    {
        /// <summary>
        /// Parses input sequence
        /// </summary>
        /// <param name="grammar">Language grammar</param>
        /// <param name="tokens">Input tokens sequence</param>
        /// <returns>Node, which represents AST's root</returns>
        /// <exception cref="SyntaxException" />
        Node Parse(Grammar.Grammar grammar, IEnumerable<Token> tokens);
    }
}
