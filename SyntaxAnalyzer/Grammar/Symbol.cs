using LexicalAnalyzer;

namespace SyntaxAnalyzer.Grammar
{
    public abstract class Symbol
    {
        public string Term { get; }

        protected Symbol(string term)
        {
            Term = term;
        }

        public override string ToString() => Term;

        public override bool Equals(object obj)
        {
            var sym = obj as Symbol;
            return sym != null && sym.Term.Equals(Term);
        }

        public override int GetHashCode() => Term.GetHashCode();
    }

    public class Terminal : Symbol
    {
        public Token Token { get; set; }

        public Terminal(string term)
            : base(term)
        {
        }

        public override string ToString()
        {
            return $"{Term}: {Token.Value}";
        }
    }

    public class NonTerminal : Symbol
    {
        public NonTerminal(string term)
            : base(term)
        {
        }
    }
}