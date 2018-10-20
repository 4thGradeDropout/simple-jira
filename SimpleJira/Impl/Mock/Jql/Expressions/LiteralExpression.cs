namespace SimpleJira.Impl.Mock.Jql.Expressions
{
    internal class LiteralExpression : IJqlClause
    {
        public object Value { get; set; }
        public LiteralExpressionQuotingType Quoting { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitLiteralExpression(this);
        }

        public override string ToString()
        {
            return Value?.ToString() ?? "EMPTY";
        }
    }
}