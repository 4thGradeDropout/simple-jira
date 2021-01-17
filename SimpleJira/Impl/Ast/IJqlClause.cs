namespace SimpleJira.Impl.Ast
{
    internal interface IJqlClause
    {
        IJqlClause Accept(JqlVisitor visitor);
    }
}