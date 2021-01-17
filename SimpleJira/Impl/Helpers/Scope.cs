using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Impl.Helpers
{
    internal class Scope
    {
        private readonly object filter;
        private readonly Action<JiraIssue> initializer;

        private static readonly ConcurrentDictionary<Type, Scope> scopes = new ConcurrentDictionary<Type, Scope>();
        private static readonly Scope defaultScope = new Scope(null, null, null);

        public Scope(object filter, Action<JiraIssue> initializer, List<PropertyInfo> properties)
        {
            this.filter = filter;
            this.initializer = initializer;
            Properties = properties;
        }

        public List<PropertyInfo> Properties { get; }

        public IQueryable<TIssue> Filter<TIssue>(IQueryable<TIssue> queryable) where TIssue : JiraIssue
        {
            return filter == null
                ? queryable
                : ((Func<IQueryable<TIssue>, IQueryable<TIssue>>) filter)(queryable);
        }

        public void Initialize(JiraIssue issue)
        {
            initializer?.Invoke(issue);
        }

        public static Scope Get<TIssue>() where TIssue : JiraIssue
        {
            return Get(typeof(TIssue));
        }

        public static Scope Get(Type issueType)
        {
            return scopes.GetOrAdd(issueType, type =>
            {
                var defineScopeType = typeof(IDefineScope<>).MakeGenericType(type);
                var scopeDefinitionType = type
                    .GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                    .SingleOrDefault(x => defineScopeType.IsAssignableFrom(x));
                if (scopeDefinitionType == null)
                    return defaultScope;
                var scopeDefinition = scopeDefinitionType.New();
                var scopeBuilder = ScopeBuilder.Create(type);
                return scopeBuilder.Build(scopeDefinition);
            });
        }
    }
}