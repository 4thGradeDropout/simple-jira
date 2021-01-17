using SimpleJira.Impl.Helpers;

namespace SimpleJira.Impl.Ast
{
    internal class IssueFunctionExpression : IJqlClause
    {
        public JqlIssueFunction Function { get; set; }
        public IJqlClause SubQuery { get; set; }
        public JqlLiteralQuotingType QuotingType { get; set; }

        public IJqlClause Accept(JqlVisitor visitor)
        {
            return visitor.VisitIssueFunction(this);
        }

        public override string ToString()
        {
            var subQuery = StringHelpers.Escape(SubQuery.ToString());
            return $"(ISSUEFUNCTION IN {JqlIssueFunctionHelpers.Format(Function)}({subQuery}))";
        }
    }
}