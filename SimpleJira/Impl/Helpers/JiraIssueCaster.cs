using System;
using System.Collections.Concurrent;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Impl.Helpers
{
    internal static class JiraIssueCaster
    {
        private static readonly ConcurrentDictionary<Type, Func<IJiraIssueFieldsController, JiraIssue>> factories =
            new ConcurrentDictionary<Type, Func<IJiraIssueFieldsController, JiraIssue>>();

        public static TIssue Cast<TIssue>(JiraIssue issue) where TIssue : JiraIssue
        {
            return (TIssue) Cast(issue, typeof(TIssue));
        }

        public static JiraIssue Cast(JiraIssue issue, Type issueType)
        {
            if (issueType == typeof(JiraIssue))
                return issue;
            var factory = factories.GetOrAdd(issueType, t =>
            {
                var ctor = t.GetConstructor(new[] {typeof(IJiraIssueFieldsController)});
                if (ctor == null)
                    throw new InvalidOperationException(
                        $"type [{t.Name}] must have constructor with single parameter which type is IJiraIssueFieldsController");
                var initialize = ReflectionHelpers.GetCompiledDelegate(ctor);
                return controller => (JiraIssue) initialize(null, new object[] {controller});
            });

            var result = factory(issue.Controller);
            result.Expand = issue.Expand;
            result.Key = issue.Key;
            result.Id = issue.Id;
            result.Self = issue.Self;
            return result;
        }
    }
}