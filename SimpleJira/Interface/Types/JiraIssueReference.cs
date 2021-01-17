using System;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Interface.Types
{
    public class JiraIssueReference : IJqlToken
    {
        public string Id { get; set; }
        public string Key { get; set; }
        public string Self { get; set; }

        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public override int GetHashCode()
        {
            return Key == null ? 0 : Key.ToLower().GetHashCode();
        }

        public string ToJqlToken()
        {
            if (!string.IsNullOrEmpty(Key))
                return Key;
            if (!string.IsNullOrEmpty(Id))
                return Id;
            return null;
        }

        public static bool operator ==(JiraIssueReference reference1, JiraIssueReference reference2)
        {
            if (ReferenceEquals(reference1, null) && ReferenceEquals(reference2, null))
                return true;
            if (ReferenceEquals(reference1, null) || ReferenceEquals(reference2, null))
                return false;
            return reference1 == reference2.Key || reference1 == reference2.Id;
        }

        public static bool operator ==(JiraIssueReference reference, JiraIssue issue)
        {
            if (ReferenceEquals(reference, null) && ReferenceEquals(issue, null))
                return true;
            if (ReferenceEquals(reference, null) || ReferenceEquals(issue, null))
                return false;
            return reference == issue.Key || reference == issue.Id;
        }

        public static bool operator ==(JiraIssueReference reference, string value)
        {
            if (ReferenceEquals(reference, null) && value == null)
                return true;
            if (ReferenceEquals(reference, null) || value == null)
                return false;
            return string.Equals(reference.Id, value, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(reference.Key, value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool operator ==(JiraIssueReference reference, object value)
        {
            if (value is string stringValue)
                return reference == stringValue;
            if (value is JiraIssue jiraIssue)
                return reference == jiraIssue;
            if (value is JiraIssueReference jiraReference)
                return reference == jiraReference;
            return ReferenceEquals(reference, value);
        }

        public static bool operator ==(JiraIssue issue, JiraIssueReference reference)
        {
            return reference == issue;
        }

        public static bool operator ==(string value, JiraIssueReference reference)
        {
            return reference == value;
        }

        public static bool operator ==(object value, JiraIssueReference reference)
        {
            return reference == value;
        }

        public static bool operator !=(JiraIssueReference reference1, JiraIssueReference reference2)
        {
            return !(reference1 == reference2);
        }

        public static bool operator !=(JiraIssueReference reference, JiraIssue issue)
        {
            return !(reference == issue);
        }

        public static bool operator !=(JiraIssueReference status, string value)
        {
            return !(status == value);
        }

        public static bool operator !=(JiraIssue issue, JiraIssueReference reference)
        {
            return !(issue == reference);
        }

        public static bool operator !=(string value, JiraIssueReference status)
        {
            return !(value == status);
        }

        public static bool operator !=(JiraIssueReference status, object value)
        {
            return !(status == value);
        }

        public static bool operator !=(object value, JiraIssueReference status)
        {
            return !(value == status);
        }

        public static JiraIssueReference FromKey(string key)
        {
            return new JiraIssueReference
            {
                Key = key
            };
        }

        public static JiraIssueReference FromId(string id)
        {
            return new JiraIssueReference
            {
                Id = id
            };
        }

        public static JiraIssueReference FromIssue(JiraIssue issue)
        {
            return issue.Reference();
        }
    }
}