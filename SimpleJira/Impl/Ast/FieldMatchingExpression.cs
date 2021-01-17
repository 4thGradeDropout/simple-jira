using System;

namespace SimpleJira.Impl.Ast
{
    internal class FieldMatchingExpression : IJqlClause
    {
        public IJqlClause Field { get; set; }
        public JqlFieldMatchingType Operator { get; set; }
        public IJqlClause Value { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitFieldMatching(this);
        }

        public override string ToString()
        {
            string op;
            switch(Operator)
            {
                case JqlFieldMatchingType.Contains:
                    op = "~";
                    break;
                case JqlFieldMatchingType.Equals:
                    op = "=";
                    break;
                case JqlFieldMatchingType.NotEquals:
                    op = "!=";
                    break;
                case JqlFieldMatchingType.Less:
                    op = "<";
                    break;
                case JqlFieldMatchingType.Greater:
                    op = ">";
                    break;
                case JqlFieldMatchingType.LessOrEquals:
                    op = "<=";
                    break;
                case JqlFieldMatchingType.GreaterOrEquals:
                    op = ">=";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return $"({Field} {op} {Value})";
        }
    }
}