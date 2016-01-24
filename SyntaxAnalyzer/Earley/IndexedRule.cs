using System.Collections.Generic;
using System.Linq;

namespace SyntaxAnalyzer.Earley
{
    class IndexedRule : Rule
    {
        public readonly int Index;

        public IndexedRule(NonTerminal name, List<Symbol> production, int index)
            : this(name, production, 0, index)
        { }

        public IndexedRule(Rule rule, int index)
            : this(rule.Name, rule.Production, index)

        { }

        private IndexedRule(NonTerminal name, List<Symbol> production, int dot, int index)
            : base(name, production, dot)
        {
            Index = index;
        }

        public IndexedRule Next(int index) => !IsFinal ? new IndexedRule(Name, Production, Dot + 1, index) : null;

        public override string ToString() => base.ToString() + $", {Index}";
    }

    static class IndexedRuleEnumerable
    {
        public static IEnumerable<IndexedRule> Indexed(this IEnumerable<Rule> rules, int index)
        {
            return rules.Select(r => new IndexedRule(r, index));
        }

        public static IEnumerable<IndexedRule> Indexed(this IEnumerable<Rule> rules)
        {
            return rules.Indexed(0);
        }
    }
}