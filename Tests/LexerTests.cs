using System.Linq;
using NUnit.Framework;
using Lexer;
using StateMachine;
using System;

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
			var conv = new RegexConverter();
			conv.Regex = re;
			Assert.AreEqual(pos, conv.Postfix);
		}

		[Test]
		[TestCase("a", "a", false)]
		[TestCase("a", "b", true)]
		[TestCase("abc", "abc", false)]
		[TestCase("abc", "abd", true)]
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
		public void RegexMachineTests(string re, string input, bool throws)
		{
			var m = new RegexMachine();
			m.AddExpression(re, re);

			TestDelegate act = () => {
				foreach (char c in input)
					m.Machine.Trigger(c);
			};

			if (throws)
				Assert.Throws(typeof(StateNotFoundException), act);
			else
				Assert.DoesNotThrow(act);
		}
	}
}

