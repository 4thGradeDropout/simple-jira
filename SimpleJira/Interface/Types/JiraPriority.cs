using System;

namespace SimpleJira.Interface.Types
{
    public class JiraPriority : IJqlToken
    {
        public string IconUrl { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Self { get; set; }

        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.ToLower().GetHashCode();
        }

        public static bool operator ==(JiraPriority priority1, JiraPriority priority2)
        {
            if (ReferenceEquals(priority1, null) && ReferenceEquals(priority2, null))
                return true;
            if (ReferenceEquals(priority1, null) || ReferenceEquals(priority2, null))
                return false;
            return priority1 == priority2.Id || priority1 == priority2.Name;
        }

        public static bool operator ==(JiraPriority priority, string value)
        {
            if (ReferenceEquals(priority, null) && value == null)
                return true;
            if (ReferenceEquals(priority, null) || value == null)
                return false;
            return string.Equals(priority.Id, value, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(priority.Name, value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool operator ==(JiraPriority reference, object value)
        {
            if (value is string stringValue)
                return reference == stringValue;
            if (value is JiraPriority jiraReference)
                return reference == jiraReference;
            return ReferenceEquals(reference, value);
        }

        public static bool operator ==(string value, JiraPriority reference)
        {
            return reference == value;
        }

        public static bool operator ==(object value, JiraPriority reference)
        {
            return reference == value;
        }

        public static bool operator !=(JiraPriority reference1, JiraPriority reference2)
        {
            return !(reference1 == reference2);
        }

        public static bool operator !=(JiraPriority status, string value)
        {
            return !(status == value);
        }

        public static bool operator !=(string value, JiraPriority status)
        {
            return !(value == status);
        }

        public static bool operator !=(JiraPriority status, object value)
        {
            return !(status == value);
        }

        public static bool operator !=(object value, JiraPriority status)
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