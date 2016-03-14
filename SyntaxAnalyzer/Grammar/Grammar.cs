using System.Collections.Generic;
using System.Linq;

namespace SyntaxAnalyzer.Grammar
{
    public class Grammar
    {
        private readonly Dictionary<NonTerminal, List<List<Symbol>>> _productions;

        public NonTerminal Start { get; set; }

        public Grammar()
        {
            _productions = new Dictionary<NonTerminal, List<List<Symbol>>>();
        }

        public void Add(NonTerminal term, IEnumerable<Symbol> production)
        {
            if (!_productions.ContainsKey(term))
                _productions.Add(term, new List<List<Symbol>>(1));
            _productions[term].Add(production as List<Symbol> ?? production.ToList());
        }

        public void Add(string term, IEnumerable<Symbol> production) 
            => Add(new NonTerminal(term), production);

        public List<Rule> GetRules(NonTerminal term)
        {
            return _productions[term].Select(p => new Rule(term, new List<Symbol>(p))).ToList();
        }

        public List<Rule> GetStart() => GetRules(Start);
    }
}
