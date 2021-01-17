using System.Collections.Generic;
using System.Linq;

namespace SimpleJira.Impl.Ast
{
    internal class InExpression : IJqlClause
    {
        public IJqlClause Field { get; set; }
        public List<IJqlClause> Values { get; set; }

        public bool Not { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitIn(this);
        }

        public override string ToString()
        {
            var list = string.Join(", ", Values.Select(x => x.ToString()));
            var not = Not ? "NOT " : "";
            return $"({Field} {not}IN ({list}))";
        }
    }
}