using System;
using SimpleJira.Impl.Helpers;

namespace SimpleJira.Impl.Ast
{
    internal class ValueExpression : IJqlClause
    {
        public JqlLiteralQuotingType? QuotingType { get; set; }
        public string Value { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitValue(this);
        }

        public override string ToString()
        {
            return QuotingType switch
            {
                JqlLiteralQuotingType.Double => StringHelpers.Escape(Value),
                JqlLiteralQuotingType.Single => "'" + Value.Replace("'", "\\'") + "'",
                null => Value,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}