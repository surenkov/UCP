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

        public void AddChild(Node child)
        {
            _children.Add(child);
            child.Parent = this;
        }

        public void AddChildren(IEnumerable<Node> children)
        {
            var cList = children as List<Node> ?? children.ToList();
            _children.AddRange(cList);
            cList.ForEach(c => c.Parent = this);
        }

        public IEnumerator<Node> GetEnumerator() => _children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
