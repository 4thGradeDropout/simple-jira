namespace SimpleJira.Impl.Mock.Jql.Expressions
{
    internal class InExpression : IJqlClause
    {
        public IJqlClause Field { get; set; }
        public IJqlClause Source { get; set; }
        public bool NotIn { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitInExpression(this);
        }
    }
}