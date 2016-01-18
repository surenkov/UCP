﻿using System;
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
        private Grammar _grammar;
       
        public Parser()
        {
        }

        public Parser(string path) : this()
        {
            LoadGrammar(path);
        }

        public Parser(Stream stream) : this()
        {
            LoadGrammar(stream);
        }

        public void LoadGrammar(string path)
        {
            LoadGrammar(new FileStream(path, FileMode.Open));
        }

        public void LoadGrammar(Stream stream)
        {
            var schemas = new XmlSchemaSet();
            schemas.Add("http://savva.moe/compiler/rules.xsd", "Schemas/GrammarSchema.xsd");
            var rules = XmlReader.Create(stream, new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                Schemas = schemas
            });

            var g = new Grammar();
            while (rules.Read())
            {
                if (!rules.IsStartElement()) continue;

                string term, product;
                switch (rules.Name)
                {
                    case "grammar":
                        continue;
                    case "start":
                        string start = rules.GetAttribute("rule");
                        if (string.IsNullOrWhiteSpace(start))
                            throw new XmlException("Start rule cannot be empty");
                        g.Start = new NonTerminal(start);
                        continue;
                    case "rule":
                        term = rules.GetAttribute("term");
                        product = rules.GetAttribute("production");
                        break;
                    default:
                        throw new XmlException($"Rules definition cannot contain '{rules.Name}' element");
                }

                if (string.IsNullOrWhiteSpace(term))
                    throw new ParserException("Rule's term cannot be empty");

                if (string.IsNullOrWhiteSpace(product))
                    throw new NotImplementedException("Epsilon-rules are yet implemented");

                var production = product
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select<string, Symbol>(t =>
                    {
                        if (t.StartsWith("$"))
                            return new Terminal(t.Substring(1));
                        return new NonTerminal(t);
                    })
                    .ToList();

                g.Add(term, production);
            }
            _grammar = g;
        }

        public Node Parse(IEnumerable<Token> tokens)
        {
            if (_grammar == null)
                throw new ParserException("Grammar must be loaded before calling Parse() method");

            var parser = new EarleyParser(_grammar);
            parser.Parse(tokens);
            return parser.BuildTree();
        }
    }
}
