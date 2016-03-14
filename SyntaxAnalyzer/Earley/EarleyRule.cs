using System.Linq;
using System.Collections.Generic;
using SyntaxAnalyzer.Grammar;

namespace SyntaxAnalyzer.Earley
{
    class EarleyRule : Rule
    {
        public readonly int Start;

        public EarleyRule(NonTerminal name, List<Symbol> production, int start)
            : this(name, production, 0, start)
        {
        }

        public EarleyRule(Rule rule, int start)
            : this(rule.Name, rule.Production, start)
        { }

        private EarleyRule(NonTerminal name, List<Symbol> production, int dot, int start)
            : base(name, production, dot)
        {
            Start = start;
        }

        public new EarleyRule Next() => Next(Start);
        public EarleyRule Next(int start) => !IsFinal ? new EarleyRule(Name, Production, Dot + 1, start) : null;

        public new EarleyRule Previous() => Previous(Start);
        public EarleyRule Previous(int start) => Dot > 0 ? new EarleyRule(Name, Production, Dot - 1, start) : null;

        public EarleyRule WithDot(int dot) => WithDot(dot, Start);
        public EarleyRule WithDot(int dot, int start) => dot >= 0 && dot < Production.Count
            ? new EarleyRule(Name, Production, dot, start)
            : null;

        public override string ToString() => base.ToString() + $", {Start}";

        public override int GetHashCode() => base.GetHashCode();

        public override bool Equals(object obj)
        {
            var rule = obj as EarleyRule;
            return rule != null
                   && Start.Equals(rule.Start)
                   && base.Equals(rule);
        }
    }

    static class EarleyEnumerable
    {
        public static IEnumerable<EarleyRule> Earley(this IEnumerable<Rule> rules, int start)
        {
            return rules.Select(r => new EarleyRule(r, start));
        }
    }
}