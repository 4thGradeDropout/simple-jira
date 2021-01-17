namespace SimpleJira.Impl.Ast
{
    internal class IsEmptyExpression : IJqlClause
    {
        public bool Not { get; set; }
        public IJqlClause Field { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitIsEmpty(this);
        }

        public override string ToString()
        {
            var not = Not ? "NOT " : "";
            return $"({Field} IS {not}EMPTY)";
        }
    }
}