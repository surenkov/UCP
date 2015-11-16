using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using LexicalAnalyzer;
using StateMachine;

namespace Tests
{
    [TestFixture]
    public class LexerTests
    {
        private Lexer _lexer;

        [TestFixtureSetUp]
        public void Init()
        {
            _lexer = new Lexer("Data/SampleGoLangLexis.xml");
        }

        [Test]
        [TestCase("abc", "ab.c.")]
        [TestCase("ab|c", "ab.c|")]
        [TestCase("ab+c", "ab+.c.")]
        [TestCase("a(bb)+c", "abb.+.c.")]
        [TestCase("a([b-d])+", "abc|d|+.")]
        [TestCase("[a-cA-C0-2]", "ab|c|A|B|C|0|1|2|")]
        [TestCase("a[a-d]", "aab|c|d|.")]
        [TestCase("a[b-c]d", "abc|.d.")]
        [TestCase("a\\|b", "a\\|.b.")]
        [TestCase("a\\|\\.b", "a\\|.\\..b.")]
        [TestCase("a\\||\\.b", "a\\|.\\.b.|")]
        public void RegexConverterTests(string re, string pos)
        {
            Assert.AreEqual(pos, InfixToPostfix.Convert(re));
        }

        [Test]
        [TestCase("a", "a", false)]
        [TestCase("a", "b", true)]
        [TestCase("abc", "abc", false)]
        [TestCase("abc", "abcd", true)]
        [TestCase("abcd", "abcd", false)]
        [TestCase("a|b", "a", false)]
        [TestCase("a|b", "b", false)]
        [TestCase("a|b", "c", true)]
        [TestCase("ab?", "a", false)]
        [TestCase("ab?", "ab", false)]
        [TestCase("ab*", "a", false)]
        [TestCase("ab*", "abbbbb", false)]
        [TestCase("ab+", "abbbbb", false)]
        [TestCase("(0|1|2|3)+", "231", false)]
        [TestCase("(0|1|2|3)+", "234", true)]
        [TestCase("private|public|protected", "private", false)]
        [TestCase("private|public|protected", "public", false)]
        [TestCase("private|public|protected", "protected", false)]
        [TestCase("private|public|protected", "virtual", true)]
        public void RegexBuilderTests(string re, string input, bool throws)
        {
            var builder = new RegexBuilder();
            builder.AddExpression(re, re);
            var a = builder.Machine.ToDFA();

            TestDelegate act = () => input.ToList().ForEach(c => a.Trigger(c));

            if (!throws)
            {
                Assert.DoesNotThrow(act);
                Assert.IsTrue(a.Current.Final);
                Assert.AreEqual(a.Name.First(), re);
            }
            else
                Assert.Throws(typeof(StateNotFoundException), act);
        }

        [TestCase("Data/GoLexisTests/NumbersTest.xml")]
        [TestCase("Data/GoLexisTests/ExpressionsTest.xml")]
        [TestCase("Data/GoLexisTests/TypesTest.xml")]
        [TestCase("Data/GoLexisTests/StringsTest.xml")]
        public void LexerTestCases(string testPath)
        {
            var testCase = XDocument.Load(testPath).Root;
            Assert.NotNull(testCase);

            var data = testCase.Element("data");
            Assert.NotNull(data);
            _lexer.SetSource(data.Value);

            var result = testCase.Element("result");
            Assert.NotNull(result);
            foreach (var node in result.Elements("token"))
            {
                Assert.True(_lexer.GetToken());
                Debug.WriteLine(_lexer.Token);

                Assert.AreEqual(_lexer.Token.Value, node.Attribute("value").Value);
                Assert.AreEqual(_lexer.Token.Type, node.Attribute("type").Value);
            }
            Assert.False(_lexer.GetToken());
        }
    }
}

