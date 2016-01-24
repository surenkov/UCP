using System.Collections;
using System.Collections.Generic;

namespace SyntaxAnalyzer.Earley
{
    using IndexedRuleSet = HashSet<IndexedRule>;

    class State : IEnumerable<Rule>
    {
        private readonly Dictionary<NonTerminal, IndexedRuleSet> _rulesByName;

        private readonly Dictionary<Symbol, IndexedRuleSet> _rulesByNextTerm;

        private readonly Dictionary<NonTerminal, IndexedRuleSet> _finalRules;

        private readonly List<IndexedRule> _orderedRules;

        public IndexedRule this[int index] => _orderedRules[index];

        public int Count => _orderedRules.Count;

        public State()
        {
            _rulesByName = new Dictionary<NonTerminal, IndexedRuleSet>(1);
            _rulesByNextTerm = new Dictionary<Symbol, IndexedRuleSet>(1);
            _finalRules = new Dictionary<NonTerminal, IndexedRuleSet>();
            _orderedRules = new List<IndexedRule>(1);
        }

        public State(IEnumerable<IndexedRule> rules)
            : this()
        {
            foreach (var rule in rules)
                Add(rule);
        }

        public bool Contains(IndexedRule rule)
        {
            var name = rule.Name;
            return _rulesByName.ContainsKey(name) && _rulesByName[name].Contains(rule);
        }

        public bool ContainsName(NonTerminal term) => _rulesByName.ContainsKey(term);

        public bool ContainsTerm(Symbol term) => _rulesByNextTerm.ContainsKey(term);

        public bool ContainsFinal(NonTerminal term) => _finalRules.ContainsKey(term);

        public IndexedRuleSet RulesByName(NonTerminal term) => GetRulesFromDictionary(_rulesByName, term);

        public IndexedRuleSet RulesByNextTerm(Symbol term) => GetRulesFromDictionary(_rulesByNextTerm, term);

        public IndexedRuleSet FinalRules(NonTerminal name) => GetRulesFromDictionary(_finalRules, name);

        private static IndexedRuleSet GetRulesFromDictionary<T>(Dictionary<T, IndexedRuleSet> dict, T term)
        {
            IndexedRuleSet rules;
            dict.TryGetValue(term, out rules);
            return rules != null ? new IndexedRuleSet(rules) : new IndexedRuleSet();
        }

        public void Add(IndexedRule rule)
        {
            if (Contains(rule))
                return;

            var name = rule.Name;
            var next = rule.NextTerm;

            _orderedRules.Add(rule);
            if (!_rulesByName.ContainsKey(name))
                _rulesByName.Add(name, new IndexedRuleSet());
            _rulesByName[name].Add(rule);

            if (next != null)
            {
                if (!_rulesByNextTerm.ContainsKey(next))
                    _rulesByNextTerm.Add(next, new IndexedRuleSet());
                _rulesByNextTerm[next].Add(rule);
            }
            else
            {
                if (!_finalRules.ContainsKey(name))
                    _finalRules.Add(name, new IndexedRuleSet());
                _finalRules[name].Add(rule);
            }
        }

        public IEnumerator<Rule> GetEnumerator() => _orderedRules.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
