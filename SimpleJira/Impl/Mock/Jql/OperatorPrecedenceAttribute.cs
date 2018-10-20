using System;

namespace SimpleJira.Impl.Mock.Jql
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