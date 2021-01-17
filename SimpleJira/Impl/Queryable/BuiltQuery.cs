using System;
using SimpleJira.Impl.Ast;

namespace SimpleJira.Impl.Queryable
{
    internal class BuiltQuery
    {
        public BuiltQuery(IJqlClause query, Projection projection, Type issueType)
        {
            Query = query;
            Projection = projection;
            IssueType = issueType;
        }

        public IJqlClause Query { get; }
        public Type IssueType { get; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public bool? Count { get; set; }
        public Projection Projection { get; }
        public bool? IsAny { get; set; }
        public JqlIssueFunction? IssueFunction { get; set; }
    }
}