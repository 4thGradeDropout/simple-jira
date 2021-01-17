using System;

namespace SimpleJira.Interface.Types
{
    public class JiraStatus : IJqlToken
    {
        public string Self { get; set; }
        public string Description { get; set; }
        public string IconUrl { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public JiraStatusCategory StatusCategory { get; set; }

        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.ToLower().GetHashCode();
        }

        public static bool operator ==(JiraStatus status1, JiraStatus status2)
        {
            if (ReferenceEquals(status1, null) && ReferenceEquals(status2, null))
                return true;
            if (ReferenceEquals(status1, null) || ReferenceEquals(status2, null))
                return false;
            return status1 == status2.Id || status1 == status2.Name;
        }

        public static bool operator ==(JiraStatus status, string value)
        {
            if (ReferenceEquals(status, null) && value == null)
                return true;
            if (ReferenceEquals(status, null) || value == null)
                return false;
            return string.Equals(status.Id, value, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(status.Name, value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool operator ==(JiraStatus status, object value)
        {
            if (value is string stringValue)
                return status == stringValue;
            if (value is JiraStatus jiraStatus)
                return status == jiraStatus;
            return ReferenceEquals(status, value);
        }

        public static bool operator ==(string value, JiraStatus status)
        {
            return status == value;
        }


        public static bool operator ==(object value, JiraStatus status)
        {
            return status == value;
        }

        public static bool operator !=(JiraStatus status1, JiraStatus status2)
        {
            return !(status1 == status2);
        }

        public static bool operator !=(JiraStatus status, string value)
        {
            return !(status == value);
        }

        public static bool operator !=(string value, JiraStatus status)
        {
            return !(value == status);
        }

        public static bool operator !=(JiraStatus status, object value)
        {
            return !(status == value);
        }

        public static bool operator !=(object value, JiraStatus status)
        {
            return !(value == status);
        }

        public override string ToString()
        {
            return Name;
        }

        public string ToJqlToken()
        {
            if (!string.IsNullOrEmpty(Id))
                return Id;
            if (!string.IsNullOrEmpty(Name))
                return Name;
            return null;
        }
    }
}