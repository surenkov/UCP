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

		[Test]
		public void NFATests()
		{
			var a = new NFA<char>();

			// "a" automaton
			a.AddTransition(a.Start, new State { Final = true }, 'a');
			a.Initial();

			a.Trigger('a');
			Assert.True(a.Current.Contains(a.LastAdded));

			a.Initial();
			Assert.Throws(typeof(StateNotFoundException), () => a.Trigger('b'));

			// "abcd" automaton
			a = new NFA<char>();
			a.AddTransition(a.Start, new State(), 'a');
			a.AddTransition(a.LastAdded, new State(), 'b');
			a.AddTransition(a.LastAdded, new State(), 'c');
			a.AddTransition(a.LastAdded, new State { Final = true }, 'd');
			a.Initial();

			foreach (char c in "abcd") {
				Assert.False(a.Current.Contains(a.LastAdded));
				a.Trigger(c);
			}
			Assert.True(a.Current.Contains(a.LastAdded));
			Assert.Throws(typeof(StateNotFoundException), () => a.Trigger('a'));
		}
	}
}
