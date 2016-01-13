using System;
using System.Collections.Generic;
using System.Linq;
using LexicalAnalyzer;

namespace SyntaxAnalyzer
{
    internal class EarleyParser
    {
        private readonly List<State> _chart;

        private readonly Parser _parser;

        public EarleyParser(Parser parser)
        {
            _chart = new List<State>();
            _parser = parser;
        }

        public void Parse(IEnumerable<Token> tokens)
        {
            _chart.Clear();
            _chart.Add(new State { _parser.Start });

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
                throw new ParserException("Next predicted symbol is terminal");

            var toAdd = _parser.GetRules(nextTerm, chartIdx);
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
                throw new ParserException("Next symbol to scan is non-terminal");

            terminal.Token = token;

            if (_chart.Count <= chartIdx + 1)
                _chart.Add(new State());
            _chart[chartIdx + 1].Add(nextRule);
        }

        private void Complete(List<Rule> rules, HashSet<Rule> used, int curIdx)
        {
            var final = rules[curIdx];
            if (!final.IsFinal)
                throw new ParserException("Rule to complete is not final");

            var toAdd = _chart[final.Index]
                .RulesByNextTerm(final.Name)
                .Where(r => !used.Contains(r))
                .Select(r => r.Next())
                .ToArray();

            rules.AddRange(toAdd);
            used.UnionWith(toAdd);
        }

        public Forest BuildParseForest()
        {
            var forest = new Forest();
            throw new NotImplementedException();
        }

        private void BuildParseForestHelper()
        {
            throw new NotImplementedException();
        }
    }
}
