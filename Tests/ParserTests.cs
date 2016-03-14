using System;
using System.IO;
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
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            _lexer = new Lexer(Path.Combine("Data", "FnSampleLexis.xml")) { YieldEndOfSource = true };
            _parser = new Parser(Path.Combine("Data", "FnSampleGrammar.xml"));
        }

        [Test]
        public void BasicExpressionTest()
        {
            _lexer.SetSource("void main() {}");
            _parser.Parse(_lexer.Omit());

            _lexer.SetSource("void main(int argc, int argv) {}");
            PrintTree(_parser.Parse(_lexer.Omit()));

            _lexer.SetSource("");
            Assert.Throws<SequenceSyntaxException>(() => _parser.Parse(_lexer.Omit()));

            _lexer.SetSource("void main");
            Assert.Throws<SequenceSyntaxException>(() => _parser.Parse(_lexer.Omit()));

            _lexer.SetSource("void main ( {}");
            Assert.Throws<SequenceSyntaxException>(() => _parser.Parse(_lexer.Omit()));

            _lexer.SetSource("void main (int, int a) {}");
            Assert.Throws<SequenceSyntaxException>(() => _parser.Parse(_lexer.Omit()));

            _lexer.SetSource("void main (int a int b) {}");
            Assert.Throws<SequenceSyntaxException>(() => _parser.Parse(_lexer.Omit()));
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
