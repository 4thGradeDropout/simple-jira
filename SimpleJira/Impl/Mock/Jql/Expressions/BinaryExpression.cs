using System;

namespace SimpleJira.Impl.Mock.Jql.Expressions
{
    internal class BinaryExpression : IJqlClause
    {
        public BinaryExpression(JqlBinaryOperator binaryOperator)
        {
            Operator = binaryOperator;
        }

        public JqlBinaryOperator Operator { get; }
        public IJqlClause Left { get; set; }
        public IJqlClause Right { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitBinaryExpression(this);
        }

        public override string ToString()
        {
            var result = Left + " " + ToString(Operator) + " " + Right;
            if (Operator == JqlBinaryOperator.And || Operator == JqlBinaryOperator.Or)
                result = "(" + result + ")";
            return result;
        }

        private static string ToString(JqlBinaryOperator @operator)
        {
            switch (@operator)
            {
                case JqlBinaryOperator.And:
                    return "AND";
                case JqlBinaryOperator.Or:
                    return "OR";
                case JqlBinaryOperator.Eq:
                    return "=";
                case JqlBinaryOperator.GreaterThan:
                    return ">";
                case JqlBinaryOperator.LessThan:
                    return "<";
                case JqlBinaryOperator.GreaterThanOrEqual:
                    return ">=";
                case JqlBinaryOperator.LessThanOrEqual:
                    return "<=";
                case JqlBinaryOperator.Contains:
                    return "~";
                case JqlBinaryOperator.NotContains:
                    return "!~";
                case JqlBinaryOperator.Neq:
                    return "!=";
                default:
                    throw new ArgumentOutOfRangeException(nameof(@operator), @operator, null);
            }
        }
    }
}