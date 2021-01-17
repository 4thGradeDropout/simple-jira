using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Impl.Queryable
{
    internal class PropertiesExtractingVisitor : RelinqExpressionVisitor
    {
        private readonly MemberAccessBuilder memberAccessBuilder;
        private readonly List<QueryField> fields = new List<QueryField>();
        private readonly List<SelectedPropertyItem> items = new List<SelectedPropertyItem>();
        private Expression xRoot;
        private bool rootIsSingleItem;
        private bool isReference;

        public PropertiesExtractingVisitor(MemberAccessBuilder memberAccessBuilder)
        {
            this.memberAccessBuilder = memberAccessBuilder;
        }

        public QueryField[] GetFields()
        {
            var result = fields.ToArray();
            fields.Clear();
            return result;
        }

        public SelectedProperty GetProperty(Expression expression)
        {
            xRoot = expression;
            rootIsSingleItem = false;
            isReference = false;
            items.Clear();
            Visit(xRoot);
            return new SelectedProperty
            {
                expression = expression,
                needLocalEval = !rootIsSingleItem,
                items = items.ToArray(),
                isReference = isReference
            };
        }

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression node)
        {
            return VisitMember(node, base.VisitQuerySourceReference);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            rootIsSingleItem = ReferenceEquals(node, xRoot);
            items.Add(new SelectedPropertyItem(node.Value, -1));
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return VisitMember(node, base.VisitMember);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(JiraIssue) && node.Method.Name == "Reference")
            {
                rootIsSingleItem = ReferenceEquals(node, xRoot);
                isReference = rootIsSingleItem;
                return node;
            }

            if (node.Method.DeclaringType == typeof(JiraCustomFieldValue) &&
                node.Method.Name == "Get")
                return VisitMember(node, base.VisitMethodCall);

            return base.VisitMethodCall(node);
        }

        private Expression VisitMember<T>(T node, Func<T, Expression> baseCaller) where T : Expression
        {
            var queryField = memberAccessBuilder.GetFieldOrNull(node);
            if (queryField == null)
                return baseCaller(node);
            rootIsSingleItem = ReferenceEquals(node, xRoot);
            var fieldIndex = -1;
            for (var i = 0; i < fields.Count; i++)
                if (fields[i].Path == queryField.Path)
                {
                    fieldIndex = i;
                    break;
                }

            if (fieldIndex < 0)
            {
                fields.Add(queryField);
                fieldIndex = fields.Count - 1;
            }

            items.Add(new SelectedPropertyItem(null, fieldIndex));
            return node;
        }
    }
}