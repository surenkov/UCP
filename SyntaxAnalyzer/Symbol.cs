using LexicalAnalyzer;

namespace SyntaxAnalyzer
{
    public abstract class Symbol
    {
        public string Term { get; }
    }

    public class Terminal : Symbol
    {
        public Token Token { get; set; }
    }

    public class NonTerminal : Symbol
    {
    }
}
