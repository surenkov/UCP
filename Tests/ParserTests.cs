using LexicalAnalyzer;
using NUnit.Framework;
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
        public void BasicExpressionTest()
        {
            _lexer.SetSource("(a + a) * a");
            _parser.Parse(_lexer);
        }
    }
}
