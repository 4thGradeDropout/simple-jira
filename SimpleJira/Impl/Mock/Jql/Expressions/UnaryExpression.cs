namespace SimpleJira.Impl.Mock.Jql.Expressions
{
    internal class UnaryExpression : IJqlClause
    {
        public UnaryOperator Operator { get; set; }
        public IJqlClause Argument { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }
    }
}