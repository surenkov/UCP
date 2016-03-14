using System;
using System.Collections.Generic;
using LexicalAnalyzer;
using SyntaxAnalyzer.AST;
using SyntaxAnalyzer.Grammar;

namespace SyntaxAnalyzer
{
    /// <summary>
    /// General parsing exception
    /// </summary>
    public class SyntaxException : Exception
    {
        public SyntaxException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Token sequence parsing exception
    /// </summary>
    public class SequenceSyntaxException : Exception
    {
        public IEnumerable<Terminal> Expected { get; internal set; }

        public Token Actual { get; internal set; } 
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
        /// <exception cref="SequenceSyntaxException" />
        Node Parse(Grammar.Grammar grammar, IEnumerable<Token> tokens);
    }
}
