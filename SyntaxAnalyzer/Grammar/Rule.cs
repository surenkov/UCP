using System.Collections.Generic;
using System.Linq;

namespace SyntaxAnalyzer.Grammar
{
    /// <summary>
    /// An LR(0) rule representation
    /// </summary>
    public class Rule
    {
        public readonly int Dot;

        public readonly NonTerminal Name;

        public Symbol NextTerm => Dot < Production.Count ? Production[Dot] : null;

        public Symbol MatchedTerm => Dot > 0 ? Production[Dot - 1] : null;

        public bool IsFinal => Dot >= Production.Count;

        public List<Symbol> Production { get; }

        public Symbol this[int index] => Production[index];

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
                   && rule.Production.Count.Equals(Production.Count)
                   && rule.Production.Zip(Production, (s1, s2) => s1 == s2).All(b => b);
        }

        public override int GetHashCode()
        {
            uint value = (uint)(Name.GetHashCode() ^ Production.Count);
            int count = Dot % 33;
            return (int)((value << count) | (value >> (32 - count)));
        }
    }
}