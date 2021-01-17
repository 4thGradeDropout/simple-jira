using System;

namespace SimpleJira.Impl.Ast
{
    internal class BinaryExpression : IJqlClause
    {
        public IJqlClause Left { get; set; }
        public JqlBinaryExpressionType Operator { get; set; }
        public IJqlClause Right { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitBinary(this);
        }

        public override string ToString()
        {
            string op;
            switch(Operator)
            {
                case JqlBinaryExpressionType.Or:
                    op = "OR";
                    break;
                case JqlBinaryExpressionType.And:
                    op = "AND";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return $"({Left} {op} {Right})";
        }
    }
}