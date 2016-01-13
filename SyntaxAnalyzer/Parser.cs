using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using LexicalAnalyzer;

namespace SyntaxAnalyzer
{
    public class ParserException : Exception
    {
        public ParserException(string message)
            : base(message)
        {
        }
    }

    public class Parser
    {
        private readonly Dictionary<string, List<List<Symbol>>> _productions;

        internal Rule Start { get; private set; }

        public Parser()
        {
            _productions = new Dictionary<string, List<List<Symbol>>>();
        }

        public Parser(string path) : this()
        {
            LoadRules(path);
        }

        public Parser(Stream stream) : this()
        {
            LoadRules(stream);
        }

        public void LoadRules(string path)
        {
            LoadRules(new FileStream(path, FileMode.Open));
        }

        public void LoadRules(Stream stream)
        {
            var schemas = new XmlSchemaSet();
            schemas.Add("http://savva.moe/compiler/rules.xsd", "Schemas/RulesSchema.xsd");
            var rules = XmlReader.Create(stream, new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = schemas
            });

            while (rules.Read())
            {
                if (!rules.IsStartElement()) continue;

                string term, product;
                switch (rules.Name)
                {
                    case "rules":
                        continue;
                    case "start":
                    case "rule":
                        term = rules.GetAttribute("term");
                        product = rules.GetAttribute("production");
                        break;
                    default:
                        throw new XmlException($"Rules definition cannot contain '{rules.Name}' element");
                }

                if (string.IsNullOrWhiteSpace(term))
                    throw new InvalidDataException("Rule's term cannot be empty");

                if (string.IsNullOrWhiteSpace(product))
                    throw new NotImplementedException("Epsilon-rules aren't yet implemented");

                var production = product
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select<string, Symbol>(t =>
                    {
                        if (t.StartsWith("$"))
                            return new Terminal(t.Substring(1));
                        return new NonTerminal(t);
                    })
                    .ToList();

                if (rules.Name != "start")
                {
                    if (!_productions.ContainsKey(term))
                        _productions.Add(term, new List<List<Symbol>>(1));
                    _productions[term].Add(production);
                }
                else
                {
                    Start = new Rule(new NonTerminal(term), production, 0);
                }
            }
        }

        public void Parse(IEnumerable<Token> tokens, bool requiredOnly = true)
        {
            var parser = new EarleyParser(this);
            parser.Parse(tokens.Where(t => t == null || t.Required || !requiredOnly));
        }

        public List<Rule> GetRules(NonTerminal term, int index)
        {
            return _productions[term.Term].Select(production => new Rule(term, production, index)).ToList();
        }

        public List<Rule> GetRules(string term, int index)
        {
            return GetRules(new NonTerminal(term), index);
        }
    }
}
