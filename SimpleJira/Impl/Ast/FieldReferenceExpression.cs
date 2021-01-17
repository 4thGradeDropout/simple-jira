using Remotion.Linq.Clauses;

namespace SimpleJira.Impl.Ast
{
    internal class FieldReferenceExpression : IJqlClause
    {
        public string Field { get; set; }

        public int? CustomId { get; set; }

        public IQuerySource Source { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitFieldReference(this);
        }

        public override string ToString()
        {
            return CustomId.HasValue ? $"CF[{CustomId}]" : Field;
        }
    }
}