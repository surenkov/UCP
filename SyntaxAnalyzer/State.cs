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

    internal class Rule
    {
        public readonly int Index;

        public readonly int Dot;

        public readonly NonTerminal Name;

        public readonly List<Symbol> Production;

        public Symbol NextTerm => Production[Index];

        public bool IsFinal => Dot >= Production.Count - 1;

        private Rule(NonTerminal name, List<Symbol> production, int dot, int index)
        {
            Name = name;
            Production = production;
            Dot = dot;
            Index = index;
        }

        public Rule(NonTerminal name, List<Symbol> production, int index)
            : this(name, production, 0, index)
        { }

        public Rule Next() => Next(Index);

        public Rule Next(int index) => IsFinal ? null : new Rule(Name, Production, Dot, index);

        public override string ToString()
        {
            return $"[{Name}] -> {{{string.Join(" ", Production.Take(Dot))} • {string.Join(" ", Production.Skip(Dot))}}}";
        }

        public override bool Equals(object obj)
        {
            var rule = obj as Rule;
            return rule != null
                   && rule.Name.Equals(Name)
                   && rule.Dot.Equals(Dot)
                   && rule.Production.Equals(Production);
        }

        public override int GetHashCode() => (Name.GetHashCode() ^ Production.Count) << (Dot | Index);
    }

    internal class State : IEnumerable<Rule>
    {
        private readonly Dictionary<NonTerminal, HashSet<Rule>> _rulesByName;

        private readonly Dictionary<Symbol, HashSet<Rule>> _rulesByNextTerm;

        private readonly HashSet<NonTerminal> _finalRules;

        public State()
        {
            _rulesByName = new Dictionary<NonTerminal, HashSet<Rule>>();
            _rulesByNextTerm = new Dictionary<Symbol, HashSet<Rule>>();
            _finalRules = new HashSet<NonTerminal>();
        }

        public bool Contains(Rule rule)
        {
            var name = rule.Name;
            return _rulesByName.ContainsKey(name) && _rulesByName[name].Contains(rule);
        }

        public bool ContainsName(NonTerminal term) => _rulesByName.ContainsKey(term);

        public bool ContainsTerm(Symbol term) => _rulesByNextTerm.ContainsKey(term);

        public bool ContainsFinal(NonTerminal term) => _finalRules.Contains(term);

        public HashSet<Rule> Rules(NonTerminal term)
        {
            HashSet<Rule> rules;
            _rulesByName.TryGetValue(term, out rules);
            return rules;
        }

        public void Add(Rule rule)
        {
            var name = rule.Name;
            var next = rule.NextTerm;

            if (!_rulesByName.ContainsKey(name))
                _rulesByName.Add(name, new HashSet<Rule>());

            if (!_rulesByNextTerm.ContainsKey(next))
                _rulesByNextTerm.Add(next, new HashSet<Rule>());

            if (rule.IsFinal)
                _finalRules.Add(rule.Name);

            _rulesByName[name].Add(rule);
            _rulesByNextTerm[next].Add(rule);
        }

        public void MergeWith(IEnumerable<Rule> rules)
        {
            foreach (var rule in rules) Add(rule);
        }

        public IEnumerator<Rule> GetEnumerator()
        {
            return _rulesByName.SelectMany(pair => pair.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
