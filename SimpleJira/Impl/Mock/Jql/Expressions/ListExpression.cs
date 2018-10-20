using System.Collections.Generic;

namespace SimpleJira.Impl.Mock.Jql.Expressions
{
    internal class ListExpression : IJqlClause
    {
        public List<IJqlClause> Elements { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitListExpression(this);
        }
    }
}