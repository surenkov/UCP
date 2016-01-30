using System;
using System.Collections.Generic;
using System.Linq;
using LexicalAnalyzer;

namespace SyntaxAnalyzer.Earley
{
    class EarleyParser : AbstractParser
    {
        private readonly List<State> _chart;

        public EarleyParser(Grammar grammar)
            : base(grammar)
        {
            _chart = new List<State>();
        }

        public override Node Parse(IEnumerable<Token> tokens)
        {
            _chart.Clear();
            _chart.Add(new State(Grammar.GetStart().Indexed()));

            var enumerator = tokens.GetEnumerator();
            for (int i = 0; enumerator.MoveNext(); i++)
            {
                var rules = _chart[i];
                for (int j = 0; j < rules.Count; j++)
                {
                    if (!rules[j].IsFinal)
                    {
                        if (rules[j].NextTerm.GetType() == typeof(NonTerminal))
                        {
                            Predict(i, j);
                        }
                        else
                        {
                            Scan(enumerator.Current, i, j);
                        }
                    }
                    else
                    {
                        Complete(i, j);
                    }
                }
            }
            enumerator.Dispose();
            return BuildTree();
        }

        private void Predict(int chartIdx, int ruleIdx)
        {
            var nextTerm = _chart[chartIdx][ruleIdx].NextTerm as NonTerminal;
            if (nextTerm == null)
                throw new SyntaxException("Next predicted symbol is terminal");

            var toAdd = Grammar.GetRules(nextTerm).Indexed(chartIdx);
            foreach (var rule in toAdd)
                _chart[chartIdx].Add(rule);
        }

        private void Scan(Token token, int chartIdx, int ruleIdx)
        {
            var rule = _chart[chartIdx][ruleIdx];
            if (token == null || rule.NextTerm.Term != token.Type)
                return;

            var nextRule = rule.Next(rule.Index);
            if (nextRule == null)
                return;

            var terminal = nextRule.MatchedTerm as Terminal;
            if (terminal == null)
                throw new SyntaxException("Next symbol to scan is non-terminal");

            terminal.Token = token;

            if (_chart.Count <= chartIdx + 1)
                _chart.Add(new State());
            _chart[chartIdx + 1].Add(nextRule);
        }

        private void Complete(int chartIdx, int ruleIdx)
        {
            var state = _chart[chartIdx];
            var final = state[ruleIdx];
            if (!final.IsFinal)
                throw new SyntaxException("Rule to complete is not final");

            var toAdd = _chart[final.Index]
                .RulesByNextTerm(final.Name)
                .Select(r => r.Next(r.Index));

            foreach (var rule in toAdd)
                state.Add(rule);
        }

        private Node BuildTree()
        {
            var gamma = _chart[_chart.Count - 1].FinalRules(Grammar.Start).FirstOrDefault();
            if (gamma == null)
                throw new SyntaxException("Input string doesn't belong to the grammar");

            throw new NotImplementedException();
        }
    }
}
