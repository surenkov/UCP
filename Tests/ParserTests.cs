﻿using System.Linq;
using LexicalAnalyzer;
using NUnit.Framework;
using SyntaxAnalyzer;

namespace Tests
{
    [TestFixture]
    public class ParserTests
    {
        private Lexer _lexer;
        private Parser _parser;

        [OneTimeSetUp]
        public void Init()
        {
            System.IO.Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
            _lexer = new Lexer("Data\\SampleExprLexis.xml") { YieldEndOfSource = true };
            _parser = new Parser("Data\\SampleExprRules.xml");
        }

        [TestCase]
        public void BasicExpressionTest()
        {
            _lexer.SetSource("2 + 3 * 4");
            _parser.Parse(_lexer.Where(t => t == null || t.Required));
        }
    }
}
