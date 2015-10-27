using System;
using System.Collections.Generic;
using NUnit.Framework;
using Lexer;

namespace Tests
{
	[TestFixture]
	public class LexerTests
	{
		[Test]
		public void RegexConverterTests()
		{
			var testData = new Dictionary<string, string> {
				{ "a.b.c", "ab.c." },
				{ "a.b|c", "ab.c|" },
				{ "a.b+.c", "ab+.c." },
				{ "a.(b.b)+.c", "abb.+.c." }
			};
			var conv = new RegexConverter();
			foreach (var pair in testData) {
				conv.Regex = pair.Key;
				Assert.AreEqual(pair.Value, conv.Postfix);
			}
		}
	}
}

