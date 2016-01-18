using System;
using System.Collections.Generic;
using System.Linq;
using LexicalAnalyzer;

namespace SyntaxAnalyzer
{
    internal class EarleyParser : AbstractParser
    {
        private readonly List<State> _chart;

        public EarleyParser(Grammar grammar)
            : base(grammar)
        {
            _chart = new List<State>();
        }

        public override void Parse(IEnumerable<Token> tokens)
        {
            _chart.Clear();
            _chart.Add(new State(Grammar.GetStart()));

            var enumerator = tokens.GetEnumerator();
            for (int i = 0; enumerator.MoveNext(); i++)
            {
                var rules = new List<Rule>(_chart[i]);
                var used = new HashSet<Rule>(_chart[i]);
                for (int j = 0; j < rules.Count; j++)
                {
                    if (!rules[j].IsFinal)
                    {
                        if (rules[j].NextTerm.GetType() == typeof(NonTerminal))
                        {
                            Predict(rules, used, j, i);
                        }
                        else
                        {
                            Scan(rules[j], enumerator.Current, i);
                        }
                    }
                    else
                    {
                        Complete(rules, used, j);
                    }
                }
                _chart[i] = new State(rules);
            }
            enumerator.Dispose();
        }

        private void Predict(List<Rule> rules, HashSet<Rule> used, int curIdx, int chartIdx)
        {
            var nextTerm = rules[curIdx]?.NextTerm as NonTerminal;
            if (nextTerm == null)
                throw new SyntaxException("Next predicted symbol is terminal");

            var toAdd = Grammar.GetRules(nextTerm, chartIdx);
            rules.AddRange(toAdd.Where(r => !used.Contains(r)));
            used.UnionWith(toAdd);
        }

        private void Scan(Rule rule, Token token, int chartIdx)
        {
            if (token == null || rule.NextTerm.Term != token.Type) return;

            var nextRule = rule.Next();
            if (nextRule == null) return;

            var terminal = nextRule.MatchedTerm as Terminal;
            if (terminal == null)
                throw new SyntaxException("Next symbol to scan is non-terminal");

            terminal.Token = token;

            if (_chart.Count <= chartIdx + 1)
                _chart.Add(new State());
            _chart[chartIdx + 1].Add(nextRule);
        }

        private void Complete(List<Rule> rules, HashSet<Rule> used, int curIdx)
        {
            var final = rules[curIdx];
            if (!final.IsFinal)
                throw new SyntaxException("Rule to complete is not final");

            var toAdd = _chart[final.Index]
                .RulesByNextTerm(final.Name)
                .Where(r => !used.Contains(r))
                .Select(r => r.Next())
                .ToArray();

            rules.AddRange(toAdd);
            used.UnionWith(toAdd);
        }

        public override Node BuildTree()
        {
            if (!_chart[_chart.Count - 1].ContainsFinal(Grammar.Start))
                throw new SyntaxException("Input string doesn't belong to the grammar");

            var finals = Grammar.GetStart().Select(r => r.Final());
            var gammas = _chart[_chart.Count - 1].FinalRules(Grammar.Start);
            gammas.IntersectWith(finals);

            return BuildTreesHelper(null, gammas.First(), _chart.Count - 1);
        }


        private Node BuildTreesHelper(Node children, Rule rule, int chartIdx)
        {
            throw new NotImplementedException();
        }
    }
}
