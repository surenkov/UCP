using System;
using System.IO;

namespace SyntaxAnalyzer
{
    public class Parser
    {
        public Parser(string path)
        {
            LoadRules(path);
        }

        public Parser(Stream stream)
        {
            LoadRules(stream);
        }

        public void LoadRules(string path)
        {
            LoadRules(new FileStream(path, FileMode.Open));
        }

        public void LoadRules(Stream stream)
        {
            throw new NotImplementedException("Implement grammar loading");
        }
    }
}
