using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SimpleJira.Impl.Mock.Jql.Expressions;
using SimpleJira.Impl.Mock.Jql.Parser;
using SimpleJira.Interface.ObjectModel;

namespace SimpleJira.Impl.Mock.Jql
{
    internal class JqlFilterBuilder : JqlVisitor
    {
        private readonly JiraMetadata metadata;

        private static readonly Regex customFieldRegex =
            new Regex("cf\\[(\\d+)\\]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly Stack<Func<JiraIssue, bool>> filters = new Stack<Func<JiraIssue, bool>>();
        private readonly Stack<string[]> inValues = new Stack<string[]>();

        private JqlFilterBuilder(JiraMetadata metadata)
        {
            this.metadata = metadata;
        }

        public override IJqlClause VisitBinaryExpression(BinaryExpression expression)
        {
            var result = base.VisitBinaryExpression(expression);
            if (expression.Operator == JqlBinaryOperator.And)
            {
                var right = filters.Pop();
                var left = filters.Pop();
                filters.Push(issue => left(issue) && right(issue));
            }
            else if (expression.Operator == JqlBinaryOperator.Or)
            {
                var right = filters.Pop();
                var left = filters.Pop();
                filters.Push(issue => left(issue) || right(issue));
            }
            else if (expression.Operator == JqlBinaryOperator.Eq)
            {
                var field = ExtractFieldName(expression.Left);
                var value = ExtractLiteral(expression.Right);
                filters.Push(issue => JqlFieldEqualityComparer.Equals(metadata, issue, field, value));
            }
            else if (expression.Operator == JqlBinaryOperator.Neq)
            {
                var field = ExtractFieldName(expression.Left);
                var value = ExtractLiteral(expression.Right);
                filters.Push(issue => !JqlFieldEqualityComparer.Equals(metadata, issue, field, value));
            }
            else
                throw new InvalidOperationException(
                    $"unsupported operator [{expression.Operator}] in expression [{expression}]");
            return result;
        }

        public override IJqlClause VisitInExpression(InExpression expression)
        {
            var result = base.VisitInExpression(expression);
            var field = ExtractFieldName(expression.Field);
            var values = inValues.Pop();
            filters.Push(issue =>
            {
                var res = false;
                var idx = 0;
                while (!res && idx < values.Length)
                {
                    res |= JqlFieldEqualityComparer.Equals(metadata, issue, field, values[idx]);
                    idx++;
                }
                return expression.NotIn ? !res : res;
            });
            return result;
        }

        public override IJqlClause VisitListExpression(ListExpression expression)
        {
            var visitListExpression = base.VisitListExpression(expression);
            var strings = new string[expression.Elements.Count];
            for (var i = 0; i < strings.Length; i++)
                strings[i] = ExtractLiteral(expression.Elements[i]);
            inValues.Push(strings);
            return visitListExpression;
        }

        private static string ExtractFieldName(IJqlClause expression)
        {
            var columnName = ExtractLiteral(expression);
            var match = customFieldRegex.Match(columnName);
            return match.Success ? "customfield_" + match.Groups[1].Value : columnName;
        }

        private static string ExtractLiteral(IJqlClause expression)
        {
            switch (expression)
            {
                case LiteralExpression literal:
                    return (string) literal.Value;
                case IdentifierExpression identifier:
                    return identifier.Value;
                default:
                    throw new InvalidOperationException($"literal wasn't extracted from [{expression}]");
            }
        }

        public static Func<JiraIssue, bool> Build(JiraMetadata metadata, string jql)
        {
            var builder = new JqlFilterBuilder(metadata);
            var jqlClause = JqlParser.Parse(jql);
            builder.Visit(jqlClause);
            if (builder.filters.Count != 1)
                throw new InvalidOperationException("jql interpretation exception");
            return builder.filters.Pop();
        }
    }
}