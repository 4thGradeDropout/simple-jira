namespace SimpleJira.Impl.Ast
{
    internal class FieldOrdering
    {
        public IJqlClause Field { get; set; }
        public JqlOrderType Order { get; set; }
    }
}