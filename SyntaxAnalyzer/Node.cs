using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SyntaxAnalyzer
{
    public class Node : IEnumerable<Node>
    {
        private readonly List<Node> _children;

        public Symbol Symbol { get; set; }

        public Node Parent { get; set; }

        public Node()
        {
            _children = new List<Node>();
        }

        public void Add(Node child)
        {
            _children.Add(child);
            child.Parent = this;
        }

        public IEnumerator<Node> GetEnumerator() => _children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
