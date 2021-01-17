using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Impl.Queryable
{
    internal class MemberAccessBuilder : RelinqExpressionVisitor
    {
        private static readonly ConcurrentDictionary<(Type type, string path), Func<JiraIssue, object>>
            compiledFieldGetters =
                new ConcurrentDictionary<(Type type, string path), Func<JiraIssue, object>>();

        private readonly IJiraMetadataProvider metadataProvider;
        private readonly List<string> members = new List<string>();
        private bool isLocal;
        private string jiraField;
        private Type parameterType;
        private Expression bodyExpression;
        private ParameterExpression parameterExpression;
        private bool needBuild;

        public MemberAccessBuilder(IJiraMetadataProvider metadataProvider)
        {
            this.metadataProvider = metadataProvider;
        }

        public QueryField GetFieldOrNull(Expression expression)
        {
            Clear();
            needBuild = false;
            Visit(expression);
            if (isLocal)
                return null;
            var cacheKey = (type: parameterType, path: string.Join(".", members));
            var getter = compiledFieldGetters.GetOrAdd(cacheKey, delegate
            {
                Clear();
                needBuild = true;
                Visit(expression);
                var lambda = Expression.Lambda<Func<JiraIssue, object>>(
                    Expression.Convert(bodyExpression, typeof(object)),
                    parameterExpression);
                return lambda.Compile();
            });
            return new QueryField(jiraField, members, getter);
        }

        public override Expression Visit(Expression node)
        {
            if (isLocal)
                return node;

            var isMembersChain = node is MemberExpression xMember && xMember.Member.MemberType == MemberTypes.Property
                                 || node is QuerySourceReferenceExpression
                                 || node.NodeType == ExpressionType.Convert
                                 || node is MethodCallExpression xMethod && (
                                     xMethod.Method.DeclaringType == typeof(JiraCustomFieldValue) &&
                                     xMethod.Method.Name == "Get"
                                     || xMethod.Method.DeclaringType == typeof(JiraIssueCustomFields) &&
                                     xMethod.Method.Name == "get_Item" && xMethod.Arguments.Count == 1);
            if (isMembersChain)
                return base.Visit(node);

            isLocal = true;
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Visit(node.Expression);
            var propertyInfo = (PropertyInfo) node.Member;
            if (needBuild)
                bodyExpression = Expression.Property(bodyExpression, propertyInfo);

            if (node.Member.DeclaringType == typeof(JiraIssue) && node.Member.Name == "CustomFields")
                return node;
            if (jiraField == null)
                jiraField = metadataProvider.GetFieldMetadata(propertyInfo).FieldName;
            members.Add(node.Member.Name);
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Visit(node.Object);
            if (node.Method.DeclaringType == typeof(JiraCustomFieldValue) &&
                node.Method.Name == "Get")
            {
                if (needBuild)
                    bodyExpression = Expression.Call(bodyExpression, node.Method, node.Arguments);
            }
            else if (node.Method.DeclaringType == typeof(JiraIssueCustomFields) &&
                     node.Method.Name == "get_Item" && node.Arguments.Count == 1)
            {
                if (needBuild)
                    bodyExpression = Expression.Call(bodyExpression, node.Method, node.Arguments);

                var fieldName = $"customfield_{((ConstantExpression) node.Arguments[0]).Value}";
                if (jiraField == null)
                    jiraField = fieldName;

                members.Add(fieldName);
            }

            return node;
        }

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            parameterType = expression.Type;
            if (needBuild)
            {
                parameterExpression = Expression.Parameter(typeof(JiraIssue), "issue");
                bodyExpression = parameterType == typeof(JiraIssue)
                    ? (Expression) parameterExpression
                    : Expression.Convert(parameterExpression, parameterType);
            }

            return expression;
        }

        private void Clear()
        {
            members.Clear();
            parameterExpression = null;
            bodyExpression = null;
            jiraField = null;
            parameterType = null;
            isLocal = false;
        }
    }
}