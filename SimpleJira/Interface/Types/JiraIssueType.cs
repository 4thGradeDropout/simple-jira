using System;

namespace SimpleJira.Interface.Types
{
    public class JiraIssueType : IJqlToken
    {
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Self { get; set; }
        public bool SubTask { get; set; }

        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.ToLower().GetHashCode();
        }

        public string ToJqlToken()
        {
            if (!string.IsNullOrEmpty(Id))
                return Id;
            if (!string.IsNullOrEmpty(Name))
                return Name;
            return null;
        }

        public static bool operator ==(JiraIssueType type, object value)
        {
            if (value is JiraIssueType jiraType)
                return type == jiraType;
            if (value is string stringValue)
                return type == stringValue;
            return ReferenceEquals(type, value);
        }

        public static bool operator ==(JiraIssueType type1, JiraIssueType type2)
        {
            if (ReferenceEquals(type1, null) && ReferenceEquals(type2, null))
                return true;
            if (ReferenceEquals(type1, null) || ReferenceEquals(type2, null))
                return false;
            return type1 == type2.Id || type1 == type2.Name;
        }

        public static bool operator ==(JiraIssueType type, string value)
        {
            if (ReferenceEquals(type, null) && value == null)
                return true;
            if (ReferenceEquals(type, null) || value == null)
                return false;
            return string.Equals(type.Id, value, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(type.Name, value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool operator ==(string value, JiraIssueType type)
        {
            return type == value;
        }

        public static bool operator ==(object value, JiraIssueType type)
        {
            return type == value;
        }

        public static bool operator !=(JiraIssueType type1, JiraIssueType type2)
        {
            return !(type1 == type2);
        }

        public static bool operator !=(JiraIssueType type, string value)
        {
            return !(type == value);
        }

        public static bool operator !=(string value, JiraIssueType type)
        {
            return !(value == type);
        }

        public static bool operator !=(JiraIssueType type, object value)
        {
            return !(type == value);
        }

        public static bool operator !=(object value, JiraIssueType type)
        {
            return !(value == type);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}