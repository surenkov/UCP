using System.Diagnostics;
using System.Linq;
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
            _lexer = new Lexer("Data/SampleSyntax.xml");
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

        [TestCase("a")]
        [TestCase("0x00A 007 990")]
        [TestCase(".0")]
        [TestCase("72.40")]
        [TestCase("a + b")]
        [TestCase("a | b")]
        [TestCase("var x interface{}")]
        [TestCase("struct {}")]
        [TestCase("type Point3D struct { x, y, z float64 }")]
        public void LexerTestCases(string source)
        {
            _lexer.SetSource(source);
            while (_lexer.GetToken())
                Debug.WriteLine($"<{_lexer.Token}> : {_lexer.TokenType}");
        }
    }
}

