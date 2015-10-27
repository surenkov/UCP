using System;
using NUnit.Framework;
using StateMachine;

namespace Tests
{
	[TestFixture]
	public class StateMachineTests
	{
		[Test]
		public void StateTests()
		{
			var s = new State();
			Assert.IsFalse(s.Start);
			Assert.IsFalse(s.Final);

			s.Name = "TK_STATE";
			Assert.AreEqual(s.Name, "TK_STATE");

			s.Start = true;
			Assert.IsTrue(s.Start);
			Assert.IsFalse(s.Final);

			s.Final = true;
			Assert.IsTrue(s.Final);
			Assert.IsTrue(s.Start);

			s.Start = false;
			Assert.IsFalse(s.Start);
			Assert.IsTrue(s.Final);

			var s2 = new State(s);
			Assert.AreEqual(s, s2);
		}
	}
}

