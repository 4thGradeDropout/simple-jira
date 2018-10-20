namespace SimpleJira.Impl.Mock.Jql.Expressions
{
    internal abstract class JqlVisitor
    {
        public virtual IJqlClause Visit(IJqlClause clause)
        {
            return clause.Accept(this);
        }

        public virtual IJqlClause VisitIsEmptyExpression(IsEmptyExpression clause)
        {
            clause.Argument = Visit(clause.Argument);
            return clause;
        }

        public virtual IJqlClause VisitListExpression(ListExpression clause)
        {
            for (var i = 0; i < clause.Elements.Count; ++i)
                clause.Elements[i] = Visit(clause.Elements[i]);
            return clause;
        }

        public virtual IJqlClause VisitInExpression(InExpression clause)
        {
            clause.Field = Visit(clause.Field);
            clause.Source = Visit(clause.Source);
            return clause;
        }


        public virtual IJqlClause VisitBinaryExpression(BinaryExpression clause)
        {
            clause.Left = Visit(clause.Left);
            clause.Right = Visit(clause.Right);
            return clause;
        }

        public virtual IJqlClause VisitUnaryExpression(UnaryExpression clause)
        {
            clause.Argument = Visit(clause.Argument);
            return clause;
        }

        public virtual IJqlClause VisitLiteralExpression(LiteralExpression clause)
        {
            return clause;
        }

        public virtual IJqlClause VisitIdentifierExpression(IdentifierExpression clause)
        {
            return clause;
        }
    }
}