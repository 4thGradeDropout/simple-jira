using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJira.Fakes.Impl.Jql.Parser;
using SimpleJira.Impl.Ast;
using SimpleJira.Interface;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Fakes.Impl.Jql.Compiler
{
    internal class JqlCompiler : JqlVisitor
    {
        private readonly IJiraMetadataProvider metadataProvider;

        private readonly Stack<Func<JiraIssueDto[], JiraIssueDto, bool>> filters =
            new Stack<Func<JiraIssueDto[], JiraIssueDto, bool>>();

        private readonly List<JqlOrdering> ordering = new List<JqlOrdering>();

        private JqlCompiler(IJiraMetadataProvider metadataProvider)
        {
            this.metadataProvider = metadataProvider;
        }

        public override IJqlClause VisitFieldMatching(FieldMatchingExpression clause)
        {
            var result = base.VisitFieldMatching(clause);
            var value = ExtractLiteral(clause.Value);
            var fieldName = ExtractFieldName(clause.Field);
            var fieldType = metadataProvider.GetFieldType(fieldName);
            switch (clause.Operator)
            {
                case JqlFieldMatchingType.Contains:
                    filters.Push(Filters.Contains(fieldName, fieldType, value));
                    break;
                case JqlFieldMatchingType.NotContains:
                    filters.Push(Filters.Not(Filters.Contains(fieldName, fieldType, value)));
                    break;
                case JqlFieldMatchingType.Equals:
                    filters.Push(Filters.Equals(fieldName, fieldType, value));
                    break;
                case JqlFieldMatchingType.NotEquals:
                    filters.Push(Filters.Not(Filters.Equals(fieldName, fieldType, value)));
                    break;
                case JqlFieldMatchingType.Greater:
                    filters.Push(Filters.Greater(fieldName, fieldType, value));
                    break;
                case JqlFieldMatchingType.GreaterOrEquals:
                    filters.Push(Filters.GreaterOrEquals(fieldName, fieldType, value));
                    break;
                case JqlFieldMatchingType.Less:
                    filters.Push(Filters.Less(fieldName, fieldType, value));
                    break;
                case JqlFieldMatchingType.LessOrEquals:
                    filters.Push(Filters.LessOrEquals(fieldName, fieldType, value));
                    break;
                default:
                    throw new JqlCompilationException($"unknown binary operator [{clause.Operator}]");
            }

            return result;
        }

        public override IJqlClause VisitUnary(UnaryExpression clause)
        {
            var result = base.VisitUnary(clause);
            var filter = filters.Pop();
            switch (clause.Operator)
            {
                case JqlUnaryExpressionType.Not:
                    filters.Push(Filters.Not(filter));
                    break;
                default:
                    throw new JqlCompilationException($"unknown unary operator [{clause.Operator}]");
            }

            return result;
        }

        public override IJqlClause VisitBinary(BinaryExpression clause)
        {
            var result = base.VisitBinary(clause);
            var right = filters.Pop();
            var left = filters.Pop();
            switch (clause.Operator)
            {
                case JqlBinaryExpressionType.Or:
                    filters.Push(Filters.Or(left, right));
                    break;
                case JqlBinaryExpressionType.And:
                    filters.Push(Filters.And(left, right));
                    break;
                default:
                    throw new JqlCompilationException($"unknown binary operator [{clause.Operator}]");
            }

            return result;
        }

        public override IJqlClause VisitIn(InExpression clause)
        {
            var result = base.VisitIn(clause);
            var values = clause.Values.Select(ExtractLiteral).ToArray();
            var fieldName = ExtractFieldName(clause.Field);
            var fieldType = metadataProvider.GetFieldType(fieldName);
            var filter = Filters.In(fieldName, fieldType, values);
            filters.Push(clause.Not ? Filters.Not(filter) : filter);
            return result;
        }

        public override IJqlClause VisitIsEmpty(IsEmptyExpression clause)
        {
            var result = base.VisitIsEmpty(clause);
            var fieldName = ExtractFieldName(clause.Field);
            var fieldType = metadataProvider.GetFieldType(fieldName);
            var filter = Filters.IsEmpty(fieldName, fieldType);
            filters.Push(clause.Not ? Filters.Not(filter) : filter);
            return result;
        }

        public override IJqlClause VisitIssueFunction(IssueFunctionExpression clause)
        {
            var result = base.VisitIssueFunction(clause);
            var filter = filters.Pop();
            switch (clause.Function)
            {
                case JqlIssueFunction.ParentsOf:
                    filters.Push(Filters.ParentsOf(filter));
                    break;
                case JqlIssueFunction.SubTasksOf:
                    filters.Push(Filters.SubTasksOf(filter));
                    break;
                default:
                    throw new JqlCompilationException($"unknown issuefunction [{clause.Function}]");
            }

            return result;
        }

        public override IJqlClause VisitCascadeOption(CascadeOptionExpression clause)
        {
            var result = base.VisitCascadeOption(clause);
            var fieldName = ExtractFieldName(clause.Field);
            var fieldType = metadataProvider.GetFieldType(fieldName);
            var values = clause.Values.Select(ExtractLiteral).ToArray();
            filters.Push(Filters.InCascadeOption(fieldName, fieldType, values));
            return result;
        }

        public override IJqlClause VisitOrderBy(OrderByExpression clause)
        {
            var result = base.VisitOrderBy(clause);
            foreach (var field in clause.Fields)
            {
                var fieldName = ExtractFieldName(field.Field);
                var fieldType = metadataProvider.GetFieldType(fieldName);
                var direction = field.Order == JqlOrderType.Asc ? JqlOrderingDirection.Asc : JqlOrderingDirection.Desc;
                ordering.Add(new JqlOrdering(fieldName, fieldType, direction));
            }

            return result;
        }

        public static JqlCommand Compile(string jql, IJiraMetadataProvider metadataProvider)
        {
            var ast = JqlParser.Parse(jql);
            if (ast == null)
                return new JqlCommand(null, null);
            var visitor = new JqlCompiler(metadataProvider);
            if (!(ast is JqlExpression jqlExpression))
                throw new JqlCompilationException($"query [{jql}] is not compiled");

            Func<JiraIssueDto[], JiraIssueDto, bool> filter = null;
            if (jqlExpression.Filter != null)
            {
                visitor.Visit(jqlExpression.Filter);
                if (visitor.filters.Count != 1)
                    throw new JqlCompilationException($"query [{jql}] is not compiled");
                filter = visitor.filters.Pop();
            }

            JqlOrdering[] ordering = null;
            if (jqlExpression.OrderBy != null)
            {
                visitor.Visit(jqlExpression.OrderBy);
                if (visitor.ordering.Count == 0)
                    throw new JqlCompilationException($"query [{jql}] is not compiled");
                ordering = visitor.ordering.ToArray();
            }

            return new JqlCommand(filter, ordering);
        }

        private static string ExtractFieldName(IJqlClause field)
        {
            return field switch
            {
                FieldReferenceExpression customField => customField.Field,
                _ => throw new JqlCompilationException($"field was not extracted from expression [{field}]")
            };
        }

        private static string ExtractLiteral(IJqlClause value)
        {
            return value switch
            {
                ValueExpression valueExpression => valueExpression.Value,
                null => null,
                _ => throw new JqlCompilationException($"field was not extracted from expression [{value}]")
            };
        }
    }
}