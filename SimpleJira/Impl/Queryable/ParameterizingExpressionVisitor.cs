using System;
using System.Linq.Expressions;
using Remotion.Linq.Parsing;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Impl.Queryable
{
    internal class ParameterizingExpressionVisitor : RelinqExpressionVisitor
    {
        private int parameterIndex;
        private ParameterExpression xParameter;

        public Expression Parameterize(Expression expression, ParameterExpression xTargetParameter)
        {
            parameterIndex = 0;
            xParameter = xTargetParameter;
            return Visit(expression);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            return EmitParameterAccess(node.Type);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return node.Method.DeclaringType == typeof(JiraCustomFieldValue) && node.Method.Name == "Get"
                ? EmitParameterAccess(node.Type)
                : base.VisitMethodCall(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return EmitParameterAccess(node.Type);
        }

        private Expression EmitParameterAccess(Type type)
        {
            var xArrayItem = Expression.ArrayIndex(xParameter, Expression.Constant(parameterIndex));
            var result = Expression.Convert(xArrayItem, type);
            parameterIndex++;
            return result;
        }
    }
}