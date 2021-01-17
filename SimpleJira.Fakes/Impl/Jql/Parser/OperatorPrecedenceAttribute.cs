using System;

namespace SimpleJira.Fakes.Impl.Jql.Parser
{
    internal class OperatorPrecedenceAttribute : Attribute
    {
        public OperatorPrecedenceAttribute(int precedence)
        {
            Precedence = precedence;
        }

        public int Precedence { get; }
    }
}