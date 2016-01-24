﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using LexicalAnalyzer;
using SyntaxAnalyzer.Earley;

namespace SyntaxAnalyzer
{
    public class ParserException : Exception
    {
        public ParserException(string message) 
            : base(message)
        {
        }
    }

    /// <summary>
    /// Parser interface, used by compiler
    /// </summary>
    public class Parser
    {
        public Grammar Grammar { get; private set; }

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
            schemas.Add("http://savva.moe/compiler/grammar.xsd", "Schemas/GrammarSchema.xsd");
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
                        term = rules.GetAttribute("name");
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
            Grammar = g;
        }

        public Node Parse(IEnumerable<Token> tokens)
        {
            if (Grammar == null)
                throw new ParserException("Grammar must be loaded before calling Parse() method");

            var parser = new EarleyParser(Grammar);
            return Parse(tokens, parser);
        }

        /// <summary>
        /// Interface for building AST from tokens squence
        /// </summary>
        /// <param name="tokens">Tokens sequence</param>
        /// <param name="parser">Custom parser</param>
        /// <returns>AST's root</returns>
        public Node Parse(IEnumerable<Token> tokens, AbstractParser parser) => parser.Parse(tokens);
    }
}
