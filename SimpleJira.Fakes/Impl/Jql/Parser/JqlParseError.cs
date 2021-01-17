using System;

namespace SimpleJira.Fakes.Impl.Jql.Parser
{
    internal class JqlParseError : Exception
    {
        public JqlParseError(string message) : base(message)
        {
        }
    }
}