using Lexer;
using StateMachine;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class LexerTests
    {

        private TestContext testContextInstance;
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }


        [TestMethod]
        public void RegexConverterTests(string re, string postfix)
        {
            var conv = new RegexConverter();
            conv.Regex = re;
            Assert.AreEqual(postfix, conv.Postfix);
        }

        [TestMethod]
        public void RegexMachineSucceessTests(string re, string input)
        {
            var m = new RegexMachine();
            m.AddExpression(re, re);
            var a = m.Machine;

            foreach (char c in input)
                a.Trigger(c);
        }

        [TestMethod]
        [ExpectedException(typeof(StateNotFoundException))]
        public void RegexMachineFailingTests(string re, string input)
        {
            var m = new RegexMachine();
            m.AddExpression(re, re);
            var a = m.Machine;

            foreach (char c in input)
                a.Trigger(c);
        }
    }
}

