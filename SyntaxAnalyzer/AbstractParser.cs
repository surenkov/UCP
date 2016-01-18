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
        protected Grammar Grammar;

        protected AbstractParser(Grammar grammar)
        {
            Grammar = grammar;
        }

        public abstract void Parse(IEnumerable<Token> tokens);

        public abstract Node BuildTree();
    }
}
