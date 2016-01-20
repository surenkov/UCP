using System;
using System.Collections.Generic;
using LexicalAnalyzer;

namespace SyntaxAnalyzer
{
    public class SyntaxException : Exception
    {
        public SyntaxException(string message)
            : base(message)
        {
        }
    }

    public abstract class AbstractParser
    {
        protected readonly Grammar Grammar;

        protected AbstractParser(Grammar grammar)
        {
            Grammar = grammar;
        }

        /// <summary>
        /// Parses input sequence
        /// </summary>
        /// <param name="tokens">Input tokens sequence</param>
        public abstract void Parse(IEnumerable<Token> tokens);

        /// <summary>
        /// Builds AST for Parse() input
        /// </summary>
        /// <returns>Node, which represents AST's root</returns>
        public abstract Node BuildTree();
    }
}
