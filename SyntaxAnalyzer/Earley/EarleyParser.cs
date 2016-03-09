using System.Linq;
using System.Collections.Generic;
using LexicalAnalyzer;
using SyntaxAnalyzer.AST;

namespace SyntaxAnalyzer.Earley
{
    using Grammar;

    class EarleyParser : IParser
    {
        private readonly List<State> _chart;

        public EarleyParser()
        {
            _chart = new List<State>();
        }

        public Node Parse(Grammar grammar, IEnumerable<Token> tokens)
        {
            _chart.Clear();
            _chart.Add(new State(grammar.GetStart().Earley(0)));

            using (var enumerator = tokens.GetEnumerator())
            {
                for (int i = 0; enumerator.MoveNext(); i++)
                {
                    var rules = _chart[i];
                    for (int j = 0; j < rules.Count; j++)
                    {
                        if (!rules[j].IsFinal)
                        {
                            if (rules[j].NextTerm.GetType() == typeof (NonTerminal))
                            {
                                Predict(grammar, i, j);
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
            }

            int n = _chart.Count - 1;
            return BuildTree(_chart[n].FinalRules(grammar.Start).First(), n);
        }

        private void Predict(Grammar grammar, int chartIdx, int ruleIdx)
        {
            var nextTerm = _chart[chartIdx][ruleIdx].NextTerm as NonTerminal;
            if (nextTerm == null)
                throw new SyntaxException("Next predicted symbol is terminal");

            var toAdd = grammar.GetRules(nextTerm).Earley(chartIdx);
            foreach (var rule in toAdd)
                _chart[chartIdx].Add(rule);
        }

        private void Scan(Token token, int chartIdx, int ruleIdx)
        {
            var rule = _chart[chartIdx][ruleIdx];
            if (token == null || rule.NextTerm.Term != token.Type)
                return;

            var nextRule = rule.Next(rule.Start);
            if (nextRule == null)
                return;

            var terminal = nextRule.MatchedTerm as Terminal;
            if (terminal == null)
                throw new SyntaxException("Next symbol to scan is non-terminal");

            nextRule.Scanned = new Terminal(terminal.Term) { Token = token };

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

            var toAdd = _chart[final.Start]
                .RulesByNextTerm(final.Name)
                .Select(r => r.Next());

            foreach (var rule in toAdd)
                state.Add(rule);
        }

        private Node BuildTree(EarleyRule rule, int stateIdx)
        {
            if (rule == null)
                throw new SyntaxException($"Attempt to complete an empty rule in #{stateIdx} state");

            var node = new Node { Symbol = rule.Name };
            int matchedIdx = rule.Dot - 1;

            while (matchedIdx >= 0)
            {
                var matchedTerm = rule[matchedIdx] as NonTerminal;
                if (matchedTerm != null)
                {
                    int idx = matchedIdx;
                    var derivatives = _chart[stateIdx]
                        .FinalRules(matchedTerm)
                        .Where(r => _chart[r.Start].Contains(rule.WithDot(idx)));

                    var rules = derivatives as IList<EarleyRule> ?? derivatives.ToList();
                    var first = BuildTree(rules.First(), stateIdx);
                    var next = first;
                    foreach (var deriv in rules.Skip(1))
                    {
                        next.Next = BuildTree(deriv, stateIdx);
                        next = next.Next;
                    }

                    node.Add(first);
                    stateIdx = rules[0].Start;
                }
                else
                {
                    node.Add(new Node { Symbol = rule.ScannedAt(matchedIdx) });
                    stateIdx--;
                }
                matchedIdx--;
            }
            return node;
        }
    }
}
