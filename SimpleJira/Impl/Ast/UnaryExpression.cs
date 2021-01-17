namespace SimpleJira.Impl.Ast
{
    internal class UnaryExpression : IJqlClause
    {
        public IJqlClause Operand { get; set; }
        public JqlUnaryExpressionType Operator { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitUnary(this);
        }

        public override string ToString()
        {
            return "(NOT " + Operand + ")";
        }
    }
}