using System;

namespace SimpleJira.Fakes.Impl.Jql.Compiler
{
    internal struct JqlOrdering
    {
        public JqlOrdering(string field, Type type, JqlOrderingDirection direction) : this()
        {
            Field = field;
            Type = type;
            Direction = direction;
        }

        public string Field { get; }
        public Type Type { get; }
        public JqlOrderingDirection Direction { get; }
    }
}