using System.Linq;

namespace SimpleJira.Impl.Ast
{
    internal class CascadeOptionExpression : IJqlClause
    {
        public IJqlClause Field { get; set; }

        public IJqlClause[] Values { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitCascadeOption(this);
        }

        public override string ToString()
        {
            var list = string.Join(",", Values.Select(x => x.ToString()));
            return $"({Field} IN CASCADEOPTION({list}))";
        }
    }
}