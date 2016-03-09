using System;
using System.Linq;
using LexicalAnalyzer;
using NUnit.Framework;
using SyntaxAnalyzer;
using SyntaxAnalyzer.AST;

namespace Tests
{
    [TestFixture]
    public class ParserTests
    {
        private Lexer _lexer;
        private Parser _parser;

        [OneTimeSetUp]
        public void Init()
        {
            System.IO.Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            _lexer = new Lexer("Data\\FnSampleLexis.xml") { YieldEndOfSource = true };
            _parser = new Parser("Data\\FnSampleGrammar.xml");
        }

        [TestCase]
        public void BasicExpressionTest()
        {
            _lexer.SetSource("void main(int argc, int argv, int aa) {}");
            PrintTree(_parser.Parse(_lexer.Where(t => t == null || t.Required)));
        }

        private void PrintTree(Node root, int tab = 0)
        {
            foreach (var item in root.Reverse())
            {
                for (int i = 0; i < tab; i++)
                    Console.Out.Write('\t');
                Console.Out.WriteLine(item);
                PrintTree(item, tab + 1);
            }
        }
    }
}
