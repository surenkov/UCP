using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lexer;

namespace Tests
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void RegexConverterTests()
        {
            var testData = new Dictionary<string, string>
            {
                { "a.b.c", "ab.c." },
                { "a.b|c", "ab.c|" },
                { "a.b+.c", "ab+.c." },
                { "a.(b.b)+.c", "abb.+.c." }
            };
            var conv = new RegexConverter();
            foreach (var pair in testData)
            {
                conv.Regex = pair.Key;
                Assert.AreEqual(pair.Value, conv.Postfix);
            }
        }
    }
}
