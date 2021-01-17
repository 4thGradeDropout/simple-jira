using System.Linq;

namespace SimpleJira.Impl.Ast
{
    internal class OrderByExpression : IJqlClause
    {
        public FieldOrdering[] Fields { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitOrderBy(this);
        }

        public override string ToString()
        {
            return "ORDER BY " + string.Join(", ", Fields
                       .Select(x => $"{x.Field} {x.Order.ToString().ToUpper()}"));
        }
    }
}