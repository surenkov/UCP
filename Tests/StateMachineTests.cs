using StateMachine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class StateMachineTests
    {
        [TestMethod]
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

        [TestMethod]
        public void NFATriggerTests()
        {
            var a = new NFA<char>();
            a.AddTransition(a.Start, new State { Final = true }, 'a');
            a.Initialize();
            a.Trigger('a');
            Assert.IsTrue(a.Current.Contains(a.LastAdded));

            a = new NFA<char>();
            a.AddTransition(a.Start, new State(), 'a');
            a.AddTransition(a.LastAdded, new State(), 'b');
            a.AddTransition(a.LastAdded, new State(), 'c');
            a.AddTransition(a.LastAdded, new State { Final = true }, 'd');
            a.Initialize();

            foreach (char c in "abcd") {
                Assert.IsFalse(a.Current.Contains(a.LastAdded));
                a.Trigger(c);
            }
            Assert.IsTrue(a.Current.Contains(a.LastAdded));
        }

        [TestMethod]
        public void DFATriggerTests()
        {
            var a = new DFA<char>();
            a.AddTransition(a.Start, new State { Final = true }, 'a');
            a.Trigger('a');
            Assert.AreEqual(a.Current, a.LastAdded);

            a = new DFA<char>();
            a.AddTransition(a.Start, new State(), 'a');
            a.AddTransition(a.LastAdded, new State(), 'b');
            a.AddTransition(a.LastAdded, new State(), 'c');
            a.AddTransition(a.LastAdded, new State { Final = true }, 'd');

            foreach (char c in "abcd") {
                Assert.AreNotEqual(a.Current, a.LastAdded);
                a.Trigger(c);
            }
            Assert.AreEqual(a.Current, a.LastAdded);
        }
    }
}
