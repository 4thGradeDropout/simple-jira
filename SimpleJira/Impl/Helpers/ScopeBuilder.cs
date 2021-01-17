using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Impl.Helpers
{
    internal abstract class ScopeBuilder
    {
        public abstract Scope Build(object definition);

        public static ScopeBuilder Create(Type issueType)
        {
            return (ScopeBuilder) typeof(ScopeBuilder<>).MakeGenericType(issueType).New();
        }
    }

    internal class ScopeBuilder<T> : ScopeBuilder, IScopeBuilder<T> where T : JiraIssue
    {
        private readonly List<PropertyInfo> properties = new List<PropertyInfo>();
        private readonly List<Expression<Func<T, bool>>> filters = new List<Expression<Func<T, bool>>>();
        private readonly List<Action<T>> initializers = new List<Action<T>>();

        public IScopeBuilder<T> Define<TField>(Expression<Func<T, TField>> expression, TField value)
        {
            var xParameter = expression.Parameters[0];
            var xBody = Expression.MakeBinary(ExpressionType.Equal, expression.Body, Expression.Constant(value));
            var xFilter = Expression.Lambda<Func<T, bool>>(xBody, xParameter);
            filters.Add(xFilter);
            var property = (PropertyInfo) ((MemberExpression) expression.Body).Member;
            properties.Add(property);
            var propertyAccessor = PropertyAccessor.Get(property);
            initializers.Add(issue => propertyAccessor.Set(issue, value));
            return this;
        }

        public override Scope Build(object definition)
        {
            ((IDefineScope<T>) definition).Build(this);
            return new Scope(GetFilter(), GetInitializer(), properties);
        }

        private Action<JiraIssue> GetInitializer()
        {
            if (initializers.Count == 0)
                return null;
            return issue =>
            {
                foreach (var initializer in initializers)
                    initializer((T) issue);
            };
        }

        private object GetFilter()
        {
            if (filters.Count == 0)
                return null;
            return (Func<IQueryable<T>, IQueryable<T>>)
                (query => filters.Aggregate(query, (res, cur) => res.Where(cur)));
        }
    }
}