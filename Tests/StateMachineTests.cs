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
            Assert.That(s.Start, Is.False);
            Assert.That(s.Final, Is.False);

            s.Start = true;
            Assert.That(s.Start, Is.True);
            Assert.That(s.Final, Is.False);

            s.Final = true;
            Assert.That(s.Start, Is.True);
            Assert.That(s.Final, Is.True);

            s.Start = false;
            Assert.That(s.Start, Is.False);
            Assert.That(s.Final, Is.True);

            var s2 = s.Clone();
            Assert.That(s2, Is.EqualTo(s));
        }

        [Test]
        public void NFATests()
        {
            var a = new NFA<char>();

            // "a" automaton
            a.AddTransition(a.Start, new State { Final = true }, 'a');
            a.Initial();

            a.Trigger('a');
            Assert.That(a.Current.Contains(a.LastAdded), Is.True);

            a.Initial();
            Assert.That(() => a.Trigger('b'), Throws.TypeOf<StateNotFoundException>());

            // "abcd" automaton
            a = new NFA<char>();
            a.AddTransition(a.Start, new State(), 'a');
            a.AddTransition(a.LastAdded, new State(), 'b');
            a.AddTransition(a.LastAdded, new State(), 'c');
            a.AddTransition(a.LastAdded, new State { Final = true }, 'd');
            a.Initial();

            foreach (char c in "abcd") {
                Assert.That(a.Current.Contains(a.LastAdded), Is.False);
                a.Trigger(c);
            }
            Assert.That(a.Current.Contains(a.LastAdded), Is.True);
            Assert.That(() => a.Trigger('b'), Throws.TypeOf<StateNotFoundException>());
        }
    }
}
