using System.Collections;
using System.Collections.Generic;
using SyntaxAnalyzer.Grammar;

namespace SyntaxAnalyzer.Earley
{
    using ItemSet = HashSet<EarleyRule>;

    class State : IEnumerable<EarleyRule>
    {
        private readonly Dictionary<NonTerminal, ItemSet> _rulesByName;

        private readonly Dictionary<Symbol, ItemSet> _rulesByNextTerm;

        private readonly Dictionary<NonTerminal, ItemSet> _finalRules;

        private readonly List<EarleyRule> _orderedRules;

        public EarleyRule this[int index] => _orderedRules[index];

        public int Count => _orderedRules.Count;

        public State()
        {
            _rulesByName = new Dictionary<NonTerminal, ItemSet>(1);
            _rulesByNextTerm = new Dictionary<Symbol, ItemSet>(1);
            _finalRules = new Dictionary<NonTerminal, ItemSet>();
            _orderedRules = new List<EarleyRule>(1);
        }

        public State(IEnumerable<EarleyRule> rules)
            : this()
        {
            foreach (var rule in rules)
                Add(rule);
        }

        public bool Contains(EarleyRule rule)
        {
            var name = rule.Name;
            return _rulesByName.ContainsKey(name) && _rulesByName[name].Contains(rule);
        }

        public bool ContainsName(NonTerminal term) => _rulesByName.ContainsKey(term);

        public bool ContainsTerm(Symbol term) => _rulesByNextTerm.ContainsKey(term);

        public bool ContainsFinal(NonTerminal term) => _finalRules.ContainsKey(term);

        public ItemSet RulesByName(NonTerminal term) => GetRulesFromDictionary(_rulesByName, term);

        public ItemSet RulesByNextTerm(Symbol term) => GetRulesFromDictionary(_rulesByNextTerm, term);

        public ItemSet FinalRules(NonTerminal name) => GetRulesFromDictionary(_finalRules, name);

        private static ItemSet GetRulesFromDictionary<T>(Dictionary<T, ItemSet> dict, T term)
        {
            ItemSet rules;
            dict.TryGetValue(term, out rules);
            return rules != null ? new ItemSet(rules) : new ItemSet();
        }

        public void Add(EarleyRule rule)
        {
            if (Contains(rule))
                return;

            var name = rule.Name;
            var next = rule.NextTerm;

            _orderedRules.Add(rule);
            if (!_rulesByName.ContainsKey(name))
                _rulesByName.Add(name, new ItemSet());
            _rulesByName[name].Add(rule);

            if (next != null)
            {
                if (!_rulesByNextTerm.ContainsKey(next))
                    _rulesByNextTerm.Add(next, new ItemSet());
                _rulesByNextTerm[next].Add(rule);
            }
            else
            {
                if (!_finalRules.ContainsKey(name))
                    _finalRules.Add(name, new ItemSet());
                _finalRules[name].Add(rule);
            }
        }

        public IEnumerator<EarleyRule> GetEnumerator() => _orderedRules.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
