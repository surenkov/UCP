using System.Collections.Generic;
using System.Linq;
using LexicalAnalyzer;

namespace SyntaxAnalyzer
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
    }

    public class NonTerminal : Symbol
    {
        public NonTerminal(string term)
            : base(term)
        {
        }
    }

    public class Rule
    {
        protected readonly int Dot;

        public readonly NonTerminal Name;

        public Symbol NextTerm => Dot < Production.Count ? Production[Dot] : null;

        public Symbol MatchedTerm => Dot > 0 ? Production[Dot - 1] : null;

        public bool IsFinal => Dot >= Production.Count;

        public List<Symbol> Production { get; }

        protected Rule(NonTerminal name, List<Symbol> production, int dot)
        {
            Name = name;
            Production = production;
            Dot = dot;
        }

        public Rule(NonTerminal name, List<Symbol> production)
            : this(name, production, 0)
        { }

        public Rule Previous() => Dot > 0 ? new Rule(Name, Production, Dot - 1) : null;

        public Rule Next() => !IsFinal ? new Rule(Name, Production, Dot + 1) : null;

        public Rule Final() => new Rule(Name, Production, Production.Count);

        public override string ToString()
        {
            return $"{Name} → {string.Join(" ", Production.Take(Dot))} • {string.Join(" ", Production.Skip(Dot))}";
        }

        public override bool Equals(object obj)
        {
            var rule = obj as Rule;
            return rule != null
                   && rule.Dot.Equals(Dot)
                   && rule.Name.Equals(Name)
                   && rule.Production.Equals(Production);
        }

        public override int GetHashCode()
        {
            uint value = (uint)(Name.GetHashCode() ^ Production.Count);
            int count = Dot % 33;
            return (int)((value << count) | (value >> (32 - count)));
        }
    }
}