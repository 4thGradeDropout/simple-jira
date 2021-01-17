using System.Linq;

namespace SimpleJira.Impl.Ast
{
    internal class JqlExpression : IJqlClause
    {
        public IJqlClause Filter { get; set; }
        public OrderByExpression OrderBy { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitJql(this);
        }

        public override string ToString()
        {
            return string.Join(" ", new[] {Filter, OrderBy}
                .Where(x => x != null)
                .Select(x => x.ToString()));
        }
    }
}