using System;
using System.Collections.Generic;
using Remotion.Linq.Clauses;
using SimpleJira.Impl.Ast;

namespace SimpleJira.Impl.Queryable
{
    internal class QueryBuilder
    {
        private readonly List<IJqlClause> filters = new List<IJqlClause>();
        private readonly List<FieldOrdering> orderings = new List<FieldOrdering>();
        private Projection projection;
        private Type issueType;
        public IQuerySource Source { get; private set; }
        public JqlIssueFunction? IssueFunction { get; set; }
        public int? Take { get; set; }
        public int? Skip { get; set; }
        public bool? Count { get; set; }
        public bool? IsAny { get; set; }

        public BuiltQuery Build()
        {
            IJqlClause query = null;
            if (filters.Count > 0)
            {
                query = filters[0];
                for (var i = 1; i < filters.Count; i++)
                {
                    query = new BinaryExpression
                    {
                        Left = query,
                        Right = filters[i],
                        Operator = JqlBinaryExpressionType.And
                    };
                }
            }

            if (orderings.Count > 0)
            {
                var orderingArray = new FieldOrdering[orderings.Count];
                for (var i = 0; i < orderings.Count; i++)
                    orderingArray[i] = orderings[i];
                query = new JqlExpression
                {
                    Filter = query,
                    OrderBy = new OrderByExpression {Fields = orderingArray}
                };
            }

            return new BuiltQuery(query, projection, issueType)
            {
                Take = Take,
                Skip = Skip,
                Count = Count,
                IsAny = IsAny,
                IssueFunction = IssueFunction
            };
        }

        public void AddFilter(IJqlClause filter)
        {
            filters.Add(filter);
        }

        public void SetProjection(Projection newProjection)
        {
            if (projection != null)
                throw new InvalidOperationException("only one select clause supported");
            projection = newProjection;
        }

        public void SetIssueType(Type type)
        {
            issueType = type;
        }

        public void SetSource(IQuerySource value)
        {
            Source = value;
        }

        public void AppendOrdering(FieldOrdering fieldOrdering)
        {
            orderings.Add(fieldOrdering);
        }
    }
}