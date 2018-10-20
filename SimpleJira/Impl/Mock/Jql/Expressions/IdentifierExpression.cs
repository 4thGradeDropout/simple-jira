namespace SimpleJira.Impl.Mock.Jql.Expressions
{
    internal class IdentifierExpression : IJqlClause
    {
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitIdentifierExpression(this);
        }
    }
}