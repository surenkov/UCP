using System.Collections;
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
        private readonly int _dot;

        private readonly List<Symbol> _production;

        public readonly int Index;

        public readonly NonTerminal Name;

        public Symbol NextTerm => _dot < _production.Count ? _production[_dot] : null;

        public Symbol MatchedTerm => _dot > 0 ? _production[_dot - 1] : null;

        public bool IsFinal => _dot >= _production.Count;

        private Rule(NonTerminal name, List<Symbol> production, int dot, int index)
        {
            Name = name;
            _production = production;
            _dot = dot;
            Index = index;
        }

        public Rule(NonTerminal name, List<Symbol> production, int index)
            : this(name, production, 0, index)
        { }

        public Rule Previous() => Previous(Index);

        public Rule Previous(int index) => _dot > 0 ? new Rule(Name, _production, _dot - 1, index) : null;

        public Rule Next() => Next(Index);

        public Rule Next(int index) => !IsFinal ? new Rule(Name, _production, _dot + 1, index) : null;

        public Rule Final() => Final(Index);

        public Rule Final(int index) => new Rule(Name, _production, _production.Count, index);

        public override string ToString()
        {
            return $"{Name} → {string.Join(" ", _production.Take(_dot))} • {string.Join(" ", _production.Skip(_dot))}, {Index}";
        }

        public override bool Equals(object obj)
        {
            var rule = obj as Rule;
            return rule != null
                   && rule._dot.Equals(_dot)
                   && rule.Name.Equals(Name)
                   && rule._production.Equals(_production);
        }

        public override int GetHashCode()
        {
            uint value = (uint)(Name.GetHashCode() ^ _production.Count);
            int count = _dot % 33;
            return (int)((value << count) | (value >> (32 - count)));
        }
    }

    public class State : IEnumerable<Rule>
    {
        private readonly Dictionary<NonTerminal, HashSet<Rule>> _rulesByName;

        private readonly Dictionary<Symbol, HashSet<Rule>> _rulesByNextTerm;

        private readonly Dictionary<NonTerminal, HashSet<Rule>> _finalRules;

        private readonly List<Rule> _orderedRules;

        public State()
        {
            _rulesByName = new Dictionary<NonTerminal, HashSet<Rule>>(1);
            _rulesByNextTerm = new Dictionary<Symbol, HashSet<Rule>>(1);
            _finalRules = new Dictionary<NonTerminal, HashSet<Rule>>();
            _orderedRules = new List<Rule>(1);
        }

        public State(IEnumerable<Rule> rules)
            : this()
        {
            foreach (var rule in rules)
                Add(rule);
        }

        public bool Contains(Rule rule)
        {
            var name = rule.Name;
            return _rulesByName.ContainsKey(name) && _rulesByName[name].Contains(rule);
        }

        public bool ContainsName(NonTerminal term) => _rulesByName.ContainsKey(term);

        public bool ContainsTerm(Symbol term) => _rulesByNextTerm.ContainsKey(term);

        public bool ContainsFinal(NonTerminal term) => _finalRules.ContainsKey(term);

        public HashSet<Rule> RulesByName(NonTerminal term) => GetRulesFromDictionary(_rulesByName, term);

        public HashSet<Rule> RulesByNextTerm(Symbol term) => GetRulesFromDictionary(_rulesByNextTerm, term);

        public HashSet<Rule> FinalRules(NonTerminal term) => GetRulesFromDictionary(_finalRules, term);

        private static HashSet<Rule> GetRulesFromDictionary<T>(Dictionary<T, HashSet<Rule>> dict, T term)
        {
            HashSet<Rule> rules;
            dict.TryGetValue(term, out rules);
            return rules != null ? new HashSet<Rule>(rules) : new HashSet<Rule>();
        }

        public void Add(Rule rule)
        {
            var name = rule.Name;
            var next = rule.NextTerm;
            _orderedRules.Add(rule);

            if (!_rulesByName.ContainsKey(name))
                _rulesByName.Add(name, new HashSet<Rule>());
            _rulesByName[name].Add(rule);

            if (next != null)
            {
                if (!_rulesByNextTerm.ContainsKey(next))
                    _rulesByNextTerm.Add(next, new HashSet<Rule>());
                _rulesByNextTerm[next].Add(rule);
            }

            if (!rule.IsFinal) return;
            if (!_finalRules.ContainsKey(name))
                _finalRules.Add(name, new HashSet<Rule>());
            _finalRules[name].Add(rule);
        }

        public IEnumerator<Rule> GetEnumerator() => _orderedRules.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
