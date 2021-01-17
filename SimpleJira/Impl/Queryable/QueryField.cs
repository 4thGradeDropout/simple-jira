using System;
using System.Collections.Generic;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Impl.Queryable
{
    internal class QueryField
    {
        private readonly Func<JiraIssue, object> getter;

        public QueryField(string fieldName,
            IEnumerable<string> pathItems,
            Func<JiraIssue, object> getter)
        {
            this.getter = getter;
            Expression = fieldName;
            Path = string.Join(".", pathItems);
        }

        public object GetValue(JiraIssue issue)
        {
            return getter(issue);
        }

        public string Path { get; }
        public string Expression { get; }
    }
}