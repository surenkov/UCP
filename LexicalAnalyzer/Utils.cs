using System;

namespace LexicalAnalyzer
{
    public class UnknownTokenException : Exception
    {
        public UnknownTokenException(string message) 
            : base(message)
        {
        }

        public UnknownTokenException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Lexer's token
    /// </summary>
    public class Token
    {
        /// <summary>
        /// Token's start line
        /// </summary>
        public uint Line { get; set; }

        /// <summary>
        /// Token's start position in line
        /// </summary>
        public uint Column { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public bool Required { get; set; }

        public override string ToString()
        {
            return $"[Ln {Line}, Col {Column}] <{Type}>: <{Value}>";
        }
    }
}