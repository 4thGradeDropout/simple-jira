using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;
using SimpleJira.Impl.Ast;
using SimpleJira.Impl.Helpers;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;
using SimpleJira.Interface.Types;
using BinaryExpression = System.Linq.Expressions.BinaryExpression;
using UnaryExpression = System.Linq.Expressions.UnaryExpression;

namespace SimpleJira.Impl.Queryable
{
    internal class FilterPredicateAnalyzer : RelinqExpressionVisitor
    {
        private readonly IJiraMetadataProvider metadataProvider;
        private readonly QueryBuilder queryBuilder;
        private readonly IQuerySource parentQuerySource;

        private static readonly Dictionary<ExpressionType, JqlFieldMatchingType> fieldMatchingOperators =
            new Dictionary<ExpressionType, JqlFieldMatchingType>
            {
                [ExpressionType.Equal] = JqlFieldMatchingType.Equals,
                [ExpressionType.NotEqual] = JqlFieldMatchingType.NotEquals,
                [ExpressionType.GreaterThan] = JqlFieldMatchingType.Greater,
                [ExpressionType.GreaterThanOrEqual] = JqlFieldMatchingType.GreaterOrEquals,
                [ExpressionType.LessThan] = JqlFieldMatchingType.Less,
                [ExpressionType.LessThanOrEqual] = JqlFieldMatchingType.LessOrEquals,
            };

        private static readonly Dictionary<ExpressionType, JqlBinaryExpressionType> logicalOperators =
            new Dictionary<ExpressionType, JqlBinaryExpressionType>
            {
                [ExpressionType.Add] = JqlBinaryExpressionType.And,
                [ExpressionType.AndAlso] = JqlBinaryExpressionType.And,
                [ExpressionType.Or] = JqlBinaryExpressionType.Or,
                [ExpressionType.OrElse] = JqlBinaryExpressionType.Or,
            };

        private readonly Stack<IJqlClause> stack = new Stack<IJqlClause>();
        private JqlIssueFunction? issueFunction;

        public FilterPredicateAnalyzer(IJiraMetadataProvider metadataProvider, QueryBuilder queryBuilder,
            IQuerySource parentQuerySource)
        {
            this.metadataProvider = metadataProvider;
            this.queryBuilder = queryBuilder;
            this.parentQuerySource = parentQuerySource;
        }

        public void Apply(Expression xTargetFilter)
        {
            stack.Clear();
            issueFunction = null;
            Visit(xTargetFilter);
            if (stack.Count > 1)
                throw new InvalidOperationException($"can't apply 'Where' operator for expression [{xTargetFilter}].");
            if (stack.Count == 0 && !issueFunction.HasValue)
                throw new InvalidOperationException($"can't apply 'Where' operator for expression [{xTargetFilter}].");
            if (stack.Count > 0)
                queryBuilder.AddFilter(stack.Pop());
            if (issueFunction.HasValue)
                queryBuilder.IssueFunction = issueFunction;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            base.VisitBinary(node);
            if (fieldMatchingOperators.TryGetValue(node.NodeType, out var fieldMatchingOperator))
            {
                var right = stack.Pop();
                var left = stack.Pop();
                var leftField = left as FieldReferenceExpression;
                var leftValue = left as ValueExpression;
                var rightField = right as FieldReferenceExpression;
                var rightValue = right as ValueExpression;
                if (leftValue != null && rightValue != null)
                    throw new InvalidOperationException(
                        $"one of expression's sides should be field reference expression, [{node}]");
                if (leftField != null && rightValue != null)
                {
                    if (rightValue.Value == null)
                        stack.Push(new IsEmptyExpression
                        {
                            Field = leftField,
                            Not = fieldMatchingOperator == JqlFieldMatchingType.NotEquals
                        });
                    else
                        stack.Push(new FieldMatchingExpression
                        {
                            Field = leftField,
                            Value = rightValue,
                            Operator = fieldMatchingOperator
                        });
                }
                else if (leftValue != null && rightField != null)
                {
                    if (leftValue.Value == null)
                        stack.Push(new IsEmptyExpression
                        {
                            Field = rightField,
                            Not = fieldMatchingOperator == JqlFieldMatchingType.NotEquals
                        });
                    else
                        stack.Push(new FieldMatchingExpression
                        {
                            Field = rightField,
                            Value = leftValue,
                            Operator = fieldMatchingOperator
                        });
                }
                else if (leftField != null && rightField != null && leftField.Source != rightField.Source)
                {
                    if (issueFunction.HasValue)
                        throw new InvalidOperationException(
                            $"only one filter which defines the parent-child relationship is allowed, [{node}]");
                    var parent = FieldBySource(parentQuerySource, leftField, rightField);
                    var child = FieldBySource(queryBuilder.Source, leftField, rightField);
                    if (parent == null || child == null)
                        throw new InvalidOperationException(
                            $"at least one of the fields has an unsupported query source, [{node}]");
                    if (fieldMatchingOperator != JqlFieldMatchingType.Equals)
                        throw new InvalidOperationException(
                            $"the parent-child relationship must be defined by [Equal] operator only, [{node}]");
                    if (string.Equals(parent.Field, "parent", StringComparison.InvariantCultureIgnoreCase))
                        issueFunction = JqlIssueFunction.SubTasksOf;
                    else if (string.Equals(child.Field, "parent", StringComparison.InvariantCultureIgnoreCase))
                        issueFunction = JqlIssueFunction.ParentsOf;
                    else
                        throw new InvalidOperationException(
                            $"the parent-child relationship must be defined using [.Parent] field, [{node}]");
                }
                else if (leftField != null && rightField != null && leftField.Source == rightField.Source)
                    throw new InvalidOperationException(
                        $"one of expression's sides should be constant expression, [{node}]");
                else
                    throw new InvalidOperationException($"unsupported binary expression, operator [{node.NodeType}]");
            }
            else if (logicalOperators.TryGetValue(node.NodeType, out var logicalOperator))
            {
                var right = stack.Pop();
                var left = stack.Pop();
                stack.Push(new Ast.BinaryExpression
                {
                    Left = left,
                    Right = right,
                    Operator = logicalOperator
                });
            }
            else
                throw new InvalidOperationException($"unsupported binary expression, operator [{node.NodeType}]");

            return node;
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            var queryModel = expression.QueryModel;
            if (IsArrayEqualExpression(expression))
            {
                var containsOperator = (ContainsResultOperator) queryModel.ResultOperators[0];
                Visit(queryModel.MainFromClause.FromExpression);
                Visit(containsOperator.Item);
                var constant = (ValueExpression) stack.Pop();
                var field = (FieldReferenceExpression) stack.Pop();
                stack.Push(new FieldMatchingExpression
                {
                    Field = field,
                    Value = constant,
                    Operator = JqlFieldMatchingType.Equals
                });
                return expression;
            }

            if (IsInExpression(expression))
            {
                var containsOperator = (ContainsResultOperator) queryModel.ResultOperators[0];
                Visit(containsOperator.Item);
                var field = (FieldReferenceExpression) stack.Pop();
                if (!(queryModel.MainFromClause.FromExpression is ConstantExpression constantExpression))
                    throw new InvalidOperationException(
                        $"'In' filter should be applied to constant expression only, [{expression}]");

                if (constantExpression.Value == null)
                    throw new InvalidOperationException($"can not apply 'In' filter to null, [{expression}]");
                List<IJqlClause> values;
                if (constantExpression.Value is IEnumerable enumerable)
                {
                    values = new List<IJqlClause>();
                    foreach (var item in enumerable)
                        values.Add(Value(item));
                }
                else
                    throw new InvalidOperationException(
                        $"'In' filter should be applied to constant expression only, [{expression}]");

                stack.Push(new InExpression
                {
                    Field = field,
                    Values = values,
                    Not = false
                });
                return expression;
            }

            var builder = new QueryBuilder();
            var visitor = new QueryModelVisitor(metadataProvider, builder, queryBuilder.Source);
            queryModel.Accept(visitor);
            var builtQuery = builder.Build();
            stack.Push(new IssueFunctionExpression
            {
                Function = builtQuery.IssueFunction.GetValueOrDie(),
                QuotingType = JqlLiteralQuotingType.Double,
                SubQuery = builtQuery.Query
            });
            return expression;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            base.VisitUnary(node);
            var filter = stack.Pop();
            if (node.NodeType == ExpressionType.Not)
            {
                stack.Push(new Ast.UnaryExpression
                {
                    Operator = JqlUnaryExpressionType.Not,
                    Operand = filter
                });
            }
            else if (node.NodeType == ExpressionType.Convert)
                stack.Push(filter);
            else
                throw new InvalidOperationException(
                    $"not supported unary operator [{node.NodeType}]");

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            base.VisitMethodCall(node);
            if (node.Method.DeclaringType == typeof(JiraCustomFieldValue) && node.Method.Name == "Get")
                return node;

            if (node.Method.DeclaringType == typeof(JiraIssueCustomFields) && node.Method.Name == "get_Item" &&
                node.Arguments.Count == 1)
            {
                var value = (ValueExpression) stack.Pop();
                var field = (FieldReferenceExpression) stack.Pop();
                field.CustomId = int.Parse(value.Value);
                stack.Push(field);
            }

            else if (node.Method.DeclaringType == typeof(JqlFunctions) && node.Method.Name == "Contains")
            {
                var constant = (ValueExpression) stack.Pop();
                var field = (FieldReferenceExpression) stack.Pop();
                stack.Push(new FieldMatchingExpression
                {
                    Field = field,
                    Operator = JqlFieldMatchingType.Contains,
                    Value = constant
                });
            }
            else
                throw new InvalidOperationException(
                    $"not supported method [{node.Method.DeclaringType}.{node.Method.Name}(...)]");

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            base.VisitMember(node);

            var property = node.Member as PropertyInfo;
            if (property == null)
                throw new InvalidOperationException(
                    $"member chain should contain only properties [{node}]");

            var filter = stack.Pop();
            if (filter is FieldReferenceExpression fieldReference)
            {
                if (fieldReference.Source == null)
                    throw new InvalidOperationException(
                        $"member chain should start from query source [{node}]");
                if (property.DeclaringType == typeof(JiraIssue) && property.Name == "CustomFields")
                {
                    stack.Push(fieldReference);
                }
                else if (property.DeclaringType == typeof(JiraStatus)
                         && property.Name == "StatusCategory"
                         && property.PropertyType == typeof(JiraStatusCategory))
                {
                    fieldReference.Field = "STATUSCATEGORY";
                    fieldReference.CustomId = null;
                    stack.Push(fieldReference);
                }
                else if (fieldReference.Field == null)
                {
                    var jiraProperty = metadataProvider.GetFieldMetadata(property);
                    fieldReference.Field = jiraProperty.FieldName;
                    fieldReference.CustomId = CustomFieldHelpers.ExtractIdentifier(jiraProperty.FieldName);
                    stack.Push(fieldReference);
                }
                else
                    stack.Push(fieldReference);
            }
            else
                throw new InvalidOperationException(
                    $"member chain should start from query source [{node}]");

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            stack.Push(Value(node.Value));
            return node;
        }

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            base.VisitQuerySourceReference(expression);
            stack.Push(new FieldReferenceExpression
            {
                Source = expression.ReferencedQuerySource
            });
            return expression;
        }

        private static bool IsArrayEqualExpression(SubQueryExpression expression)
        {
            var queryModel = expression.QueryModel;
            if (queryModel.BodyClauses.Count != 0)
                return false;
            if (queryModel.ResultOperators.Count != 1)
                return false;
            if (!(queryModel.ResultOperators[0] is ContainsResultOperator))
                return false;
            return queryModel.MainFromClause.FromExpression is MemberExpression;
        }

        private static bool IsInExpression(SubQueryExpression expression)
        {
            var queryModel = expression.QueryModel;
            if (queryModel.BodyClauses.Count != 0)
                return false;
            if (queryModel.ResultOperators.Count != 1)
                return false;
            if (!(queryModel.ResultOperators[0] is ContainsResultOperator))
                return false;
            return queryModel.MainFromClause.FromExpression is ConstantExpression;
        }

        private static ValueExpression Value(object nodeValue)
        {
            var value = nodeValue switch
            {
                null => new ValueExpression(),
                string sValue => new ValueExpression {Value = sValue, QuotingType = JqlLiteralQuotingType.Double},
                int iValue => new ValueExpression {Value = iValue.ToString(), QuotingType = null},
                long lValue => new ValueExpression {Value = lValue.ToString(), QuotingType = null},
                decimal decValue => new ValueExpression
                    {Value = decValue.ToString(CultureInfo.InvariantCulture), QuotingType = null},
                float fValue => new ValueExpression
                    {Value = fValue.ToString(CultureInfo.InvariantCulture), QuotingType = null},
                double dValue => new ValueExpression
                    {Value = dValue.ToString(CultureInfo.InvariantCulture), QuotingType = null},
                DateTime dateTime => new ValueExpression
                    {Value = FilterFormatHelpers.Format(dateTime), QuotingType = JqlLiteralQuotingType.Single},
                IJqlToken token => new ValueExpression
                    {Value = token.ToJqlToken(), QuotingType = JqlLiteralQuotingType.Double},
                _ => throw new InvalidOperationException($"unknown property type is [{nodeValue}]")
            };
            return value;
        }

        private static FieldReferenceExpression FieldBySource(IQuerySource source,
            params FieldReferenceExpression[] fields)
        {
            return fields.SingleOrDefault(x => x.Source == source);
        }
    }
}