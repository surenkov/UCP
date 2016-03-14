using NUnit.Framework;
using StateMachine;
using StateMachine.Automata;

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
        public void StatesEqualityTests()
        {
            var s1 = new States
            {
                new State(1),
                new State(2),
                new State(438783),
                new State(4),
                new State(10010),
            }.ReHash();

            var s2 = new States
            {
                new State(1),
                new State(2),
                new State(438783),
                new State(4),
                new State(10010),
            }.ReHash();

            var s3 = new States
            {
                new State(1),
                new State(2),
                new State(438783),
                new State(5),
                new State(10010),
            }.ReHash();

            Assert.That(s1.GetHashCode(), Is.EqualTo(s2.GetHashCode()));
            Assert.That(s1, Is.EqualTo(s2));
            Assert.That(s1.GetHashCode(), Is.Not.EqualTo(s3.GetHashCode()));
            Assert.That(s1, Is.Not.EqualTo(s3));
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
