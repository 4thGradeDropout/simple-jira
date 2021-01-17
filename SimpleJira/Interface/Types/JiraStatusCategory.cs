using System;

namespace SimpleJira.Interface.Types
{
    public class JiraStatusCategory : IJqlToken
    {
        public string Self { get; set; }
        public int Id { get; set; }
        public string Key { get; set; }
        public string ColorName { get; set; }
        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public override int GetHashCode()
        {
            return Key == null ? 0 : Key.ToLower().GetHashCode();
        }

        public static bool operator ==(JiraStatusCategory status1, JiraStatusCategory status2)
        {
            if (ReferenceEquals(status1, null) && ReferenceEquals(status2, null))
                return true;
            if (ReferenceEquals(status1, null) || ReferenceEquals(status2, null))
                return false;
            return status1 == status2.Key || status1 == status2.Id || status1 == status2.Name;
        }

        public static bool operator ==(JiraStatusCategory status, string value)
        {
            if (ReferenceEquals(status, null) && value == null)
                return true;
            if (ReferenceEquals(status, null) || value == null)
                return false;
            return string.Equals(status.Key, value, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(status.Id.ToString(), value, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(status.Name, value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool operator ==(JiraStatusCategory status, int value)
        {
            if (ReferenceEquals(status, null))
                return false;
            return status.Id == value;
        }

        public static bool operator ==(JiraStatusCategory status, object value)
        {
            if (value is string stringValue)
                return status == stringValue;
            if (value is int intValue)
                return status == intValue;
            if (value is JiraStatusCategory jiraStatusCategory)
                return status == jiraStatusCategory;
            return ReferenceEquals(status, value);
        }

        public static bool operator ==(string value, JiraStatusCategory status)
        {
            return status == value;
        }

        public static bool operator ==(int value, JiraStatusCategory status)
        {
            return status == value;
        }

        public static bool operator ==(object value, JiraStatusCategory status)
        {
            return status == value;
        }

        public static bool operator !=(JiraStatusCategory status1, JiraStatusCategory status2)
        {
            return !(status1 == status2);
        }

        public static bool operator !=(JiraStatusCategory status, string value)
        {
            return !(status == value);
        }

        public static bool operator !=(string value, JiraStatusCategory status)
        {
            return !(value == status);
        }

        public static bool operator !=(JiraStatusCategory status, int value)
        {
            return !(value == status);
        }

        public static bool operator !=(int value, JiraStatusCategory status)
        {
            return !(value == status);
        }

        public static bool operator !=(JiraStatusCategory status, object value)
        {
            return !(status == value);
        }

        public static bool operator !=(object value, JiraStatusCategory status)
        {
            return !(value == status);
        }

        public override string ToString()
        {
            return Name;
        }

        public string ToJqlToken()
        {
            if (!string.IsNullOrEmpty(Key))
                return Key;
            if (!string.IsNullOrEmpty(Name))
                return Name;
            return null;
        }
    }
}