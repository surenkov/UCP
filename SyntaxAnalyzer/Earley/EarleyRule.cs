using System.Linq;
using System.Collections.Generic;
using SyntaxAnalyzer.Grammar;

namespace SyntaxAnalyzer.Earley
{
    class EarleyRule : Rule
    {
        public readonly int Start;

        public EarleyRule(NonTerminal name, List<Symbol> production, int start)
            : this(name, production, 0, null, start)
        {
            _scanned = new List<Terminal>(production.Count);
            _scanned.AddRange(production.Select<Symbol, Terminal>(s => null));
        }

        public EarleyRule(Rule rule, int start)
            : this(rule.Name, rule.Production, start)
        { }

        private EarleyRule(NonTerminal name, List<Symbol> production, int dot, List<Terminal> scanned, int start)
            : base(name, production, dot)
        {
            Start = start;
            _scanned = scanned;
        }

        public new EarleyRule Next() => Next(Start);
        public EarleyRule Next(int start) => !IsFinal ? new EarleyRule(Name, Production, Dot + 1, _scanned, start) : null;

        public new EarleyRule Previous() => Previous(Start);
        public EarleyRule Previous(int start) => Dot > 0 ? new EarleyRule(Name, Production, Dot - 1, _scanned, start) : null;

        public EarleyRule WithDot(int dot) => WithDot(dot, Start);
        public EarleyRule WithDot(int dot, int start) => dot >= 0 && dot < Production.Count
            ? new EarleyRule(Name, Production, dot, _scanned, start)
            : null;

        private readonly List<Terminal> _scanned;

        public Terminal Scanned
        {
            get { return _scanned[Dot - 1]; }
            set { _scanned[Dot - 1] = value; }
        }

        public Terminal ScannedAt(int index) => _scanned[index];

        public override string ToString() => base.ToString() + $", {Start}";
    }

    static class EarleyEnumerable
    {
        public static IEnumerable<EarleyRule> Earley(this IEnumerable<Rule> rules, int start)
        {
            return rules.Select(r => new EarleyRule(r, start));
        }
    }
}