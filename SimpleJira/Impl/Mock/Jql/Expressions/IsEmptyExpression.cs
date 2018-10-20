namespace SimpleJira.Impl.Mock.Jql.Expressions
{
    internal class IsEmptyExpression : IJqlClause
    {
        public IJqlClause Argument { get; set; }
        public bool IsNotEmpty { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitIsEmptyExpression(this);
        }
    }
}