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
        [Test]
        [TestCase("abc", "ab.c.")]
        [TestCase("ab|c", "ab.c|")]
        [TestCase("ab+c", "ab+.c.")]
        [TestCase("a(bb)+c", "abb.+.c.")]
        public void RegexConverterTests(string re, string pos)
        {
            Assert.AreEqual(pos, RegexConverter.Postfix(re));
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

            if (throws)
                Assert.Throws(typeof(StateNotFoundException), act);
            else
            {
                Assert.DoesNotThrow(act);
                Assert.IsTrue(a.Current.Final);
                Assert.AreEqual(a.Name, re);
            }
        }

        [TestCase("Data/SampleSyntax.xml")]
        public void LexerTestCases(string path)
        {
            var lexer = new Lexer();
            lexer.LoadLexis(path);
            lexer.SetSource("var 111   = 222;");
            while (lexer.GetToken())
                Debug.WriteLine($"<{lexer.Token}> : <{lexer.TokenType}>");
        }
    }
}

