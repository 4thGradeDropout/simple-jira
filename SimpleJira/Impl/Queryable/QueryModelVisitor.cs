using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using SimpleJira.Impl.Ast;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Impl.Queryable
{
    internal class QueryModelVisitor : QueryModelVisitorBase
    {
        private readonly QueryBuilder queryBuilder;
        private readonly FilterPredicateAnalyzer filterPredicateAnalyzer;
        private readonly PropertiesExtractingVisitor propertiesExtractor;
        private readonly MemberAccessBuilder memberAccessBuilder;
        private readonly bool isSubQuery;

        public QueryModelVisitor(IJiraMetadataProvider metadataProvider, QueryBuilder queryBuilder,
            IQuerySource parentSource)
        {
            this.queryBuilder = queryBuilder;
            filterPredicateAnalyzer = new FilterPredicateAnalyzer(metadataProvider, queryBuilder, parentSource);
            memberAccessBuilder = new MemberAccessBuilder(metadataProvider);
            propertiesExtractor = new PropertiesExtractingVisitor(memberAccessBuilder);
            isSubQuery = parentSource != null;
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            base.VisitWhereClause(whereClause, queryModel, index);
            filterPredicateAnalyzer.Apply(whereClause.Predicate);
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            base.VisitSelectClause(selectClause, queryModel);

            var xSelector = selectClause.Selector;
            if (xSelector is QuerySourceReferenceExpression)
                return;
            var xMemberInit = xSelector as MemberInitExpression;
            MemberInfo[] members;
            SelectedProperty[] properties;
            NewExpression xNew;
            SelectedProperty[] ctorProperties = null;
            if (xMemberInit != null)
            {
                members = new MemberInfo[xMemberInit.Bindings.Count];
                properties = new SelectedProperty[xMemberInit.Bindings.Count];
                for (var i = 0; i < xMemberInit.Bindings.Count; i++)
                {
                    var binding = xMemberInit.Bindings[i];
                    if (binding.BindingType != MemberBindingType.Assignment)
                    {
                        const string messageFormat = "unexpected binding type [{0}] for member [{1}], selector [{2}]";
                        throw new InvalidOperationException(string.Format(messageFormat, binding.BindingType,
                            binding.Member.Name, xSelector));
                    }

                    var memberAssignment = (MemberAssignment) binding;
                    properties[i] = propertiesExtractor.GetProperty(memberAssignment.Expression);
                    members[i] = binding.Member;
                }

                xNew = xMemberInit.NewExpression;
            }
            else
            {
                xNew = xSelector as NewExpression;
                members = null;
                properties = xNew == null ? new[] {propertiesExtractor.GetProperty(xSelector)} : null;
            }

            if (xNew != null)
            {
                ctorProperties = new SelectedProperty[xNew.Arguments.Count];
                for (var i = 0; i < ctorProperties.Length; i++)
                    ctorProperties[i] = propertiesExtractor.GetProperty(xNew.Arguments[i]);
            }

            queryBuilder.SetProjection(new Projection
            {
                fields = propertiesExtractor.GetFields(),
                properties = properties,
                ctorProperties = ctorProperties,
                ctor = xNew?.Constructor,
                initMembers = members
            });
        }

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            base.VisitMainFromClause(fromClause, queryModel);
            queryBuilder.SetSource(queryModel.MainFromClause);
            queryBuilder.SetIssueType(queryModel.MainFromClause.ItemType);
        }

        public override void VisitOrdering(Ordering ordering, QueryModel queryModel,
            OrderByClause orderByClause, int index)
        {
            base.VisitOrdering(ordering, queryModel, orderByClause, index);
            var field = memberAccessBuilder.GetFieldOrNull(ordering.Expression);
            if (field == null)
                throw new InvalidOperationException("order by must have field reference");

            var fieldOrdering = new FieldOrdering
            {
                Field = new FieldReferenceExpression
                {
                    Field = field.Expression,
                    CustomId = CustomFieldHelpers.ExtractIdentifier(field.Expression)
                },
                Order = ordering.OrderingDirection == OrderingDirection.Asc
                    ? JqlOrderType.Asc
                    : JqlOrderType.Desc
            };
            queryBuilder.AppendOrdering(fieldOrdering);
        }

        protected override void VisitResultOperators(ObservableCollection<ResultOperatorBase> resultOperators,
            QueryModel queryModel)
        {
            if (isSubQuery)
            {
                if (resultOperators.Count != 1 || !(resultOperators[0] is AnyResultOperator))
                    throw new InvalidOperationException("subquery must have .Any() expression");
            }
            else
            {
                foreach (var o in resultOperators)
                    switch (o)
                    {
                        case TakeResultOperator takeOperator:
                            queryBuilder.Take = takeOperator.GetConstantCount();
                            break;
                        case SkipResultOperator skipResultOperator:
                            queryBuilder.Skip = skipResultOperator.GetConstantCount();
                            break;
                        case FirstResultOperator _:
                            queryBuilder.Take = 1;
                            break;
                        case SingleResultOperator _:
                            queryBuilder.Take = 2;
                            break;
                        case CountResultOperator _:
                            queryBuilder.Count = true;
                            break;
                        case AnyResultOperator _:
                            queryBuilder.IsAny = true;
                            break;
                    }
            }

            base.VisitResultOperators(resultOperators, queryModel);
        }
    }
}