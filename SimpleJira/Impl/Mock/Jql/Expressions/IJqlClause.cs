namespace SimpleJira.Impl.Mock.Jql.Expressions
{
    internal interface IJqlClause
    {
        IJqlClause Accept(JqlVisitor visitor);
    }
}