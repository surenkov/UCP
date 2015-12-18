using System.Collections.Generic;

namespace SyntaxAnalyzer
{
    internal class Rule
    {
        public int DotPosition;

        public NonTerminal Name;

        public List<Symbol> Productions;
    }
}
