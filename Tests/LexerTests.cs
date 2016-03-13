using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using LexicalAnalyzer;
using LexicalAnalyzer.Regex;
using StateMachine;

namespace Tests
{
    [TestFixture]
    public class LexerTests
    {
        private Lexer _lexer;

        [OneTimeSetUp]
        public void Init()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            _lexer = new Lexer(Path.Combine("Data", "GoLangLexis.xml"));
        }

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
            Assert.That(InfixToPostfix.Convert(re), Is.EqualTo(pos));
        }

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
                Assert.That(act, Throws.Nothing);
                Assert.That(a.Current.Final, Is.True);
                Assert.That(a.Name.First(), Is.EqualTo(re));
            }
            else
                Assert.That(act, Throws.TypeOf<StateNotFoundException>());
        }

        [TestCase("Data/GoLexisTests/NumbersTest.xml")]
        [TestCase("Data/GoLexisTests/ExpressionsTest.xml")]
        [TestCase("Data/GoLexisTests/TypesTest.xml")]
        [TestCase("Data/GoLexisTests/StringsTest.xml")]
        public void LexerTestCases(string testPath)
        {
            var testCase = XDocument.Load(testPath).Root;
            Assert.That(testCase, Is.Not.Null);

            var data = testCase.Element("data");
            Assert.That(data, Is.Not.Null);
            _lexer.SetSource(data.Value);

            var result = testCase.Element("result");
            Assert.NotNull(result);
            foreach (var node in result.Elements("token"))
            {
                Assert.That(_lexer.GetToken(), Is.True);
                Debug.WriteLine(_lexer.Token);

                Assert.That(_lexer.Token.Value, Is.EqualTo(node.Attribute("value").Value));
                Assert.That(_lexer.Token.Type, Is.EqualTo(node.Attribute("type").Value));
            }
            Assert.That(_lexer.GetToken, Is.False);
        }

        [Test]
        public void BasicLexerTestCase()
        {
            _lexer.SetSource("\n\t\t\t\t\t\t\t     ;\n  /* Multiline \n\n\n comment */ " +
                             "  \t a = b & c | d && e; // And inline comment \n a + b" +
                             "// Closing comment, why not (to test EOS)?");
            while (_lexer.GetToken())
                Debug.WriteLine(_lexer.Token);
        }
    }
}

