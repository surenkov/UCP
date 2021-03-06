﻿using System.Collections;
using System.Collections.Generic;
using SyntaxAnalyzer.Grammar;

namespace SyntaxAnalyzer.AST
{
    public class Node : IEnumerable<Node>
    {
        private readonly List<Node> _children;

        public Symbol Symbol { get; set; }

        public Node Next { get; set; }

        public Node()
        {
            _children = new List<Node>();
        }

        public void Add(Node child)
        {
            _children.Add(child);
        }

        public override string ToString() => (Symbol as Terminal)?.Token.ToString() ?? Symbol.ToString();

        public IEnumerator<Node> GetEnumerator()
        {
            if (Next != null)
                throw new ParserException("Ambigious grammar, multiple derivations had been found");
            return _children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
