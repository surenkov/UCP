using NUnit.Framework;
using LexicalAnalyzer;
using SyntaxAnalyzer;

namespace Tests
{
    [TestFixture]
    public class ParserTests
    {
        private Lexer _lexer;
        private Parser _parser;

        [TestFixtureSetUp]
        public void Init()
        {
            _lexer = new Lexer("Data/SampleExprLexis.xml") { YieldEndOfSource = true };
            _parser = new Parser("Data/SampleExprRules.xml");
        }

        [TestCase]
        [TestCase]
        [TestCase]
        [TestCase]
        [TestCase]
        public void BasicExpressionTest()
        {
            _lexer.SetSource("2 + 3 * 4");
            _parser.Parse(_lexer);
        }
    }
}
