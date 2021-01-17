using System;

namespace SimpleJira.Interface.Types
{
    public class JiraUser : IJqlToken
    {
        public bool Active { get; set; }
        public JiraAvatarUrls AvatarUrls { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Self { get; set; }
        public string TimeZone { get; set; }

        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public override int GetHashCode()
        {
            return Key == null ? 0 : Key.ToLower().GetHashCode();
        }

        public static bool operator ==(JiraUser user1, JiraUser user2)
        {
            if (ReferenceEquals(user1, null) && ReferenceEquals(user2, null))
                return true;
            if (ReferenceEquals(user1, null) || ReferenceEquals(user2, null))
                return false;
            return user1 == user2.Key || user1 == user2.Name;
        }

        public static bool operator ==(JiraUser user, string value)
        {
            if (ReferenceEquals(user, null) && value == null)
                return true;
            if (ReferenceEquals(user, null) || value == null)
                return false;
            return string.Equals(user.Key, value, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(user.Name, value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool operator ==(JiraUser user, object value)
        {
            if (value is string stringValue)
                return user == stringValue;
            if (value is JiraUser jiraUser)
                return user == jiraUser;
            return ReferenceEquals(user, value);
        }

        public static bool operator ==(string value, JiraUser user)
        {
            return user == value;
        }

        public static bool operator ==(object value, JiraUser user)
        {
            return user == value;
        }

        public static bool operator !=(JiraUser user1, JiraUser user2)
        {
            return !(user1 == user2);
        }

        public static bool operator !=(JiraUser user, string value)
        {
            return !(user == value);
        }

        public static bool operator !=(string value, JiraUser user)
        {
            return !(value == user);
        }

        public static bool operator !=(JiraUser user, object value)
        {
            return !(user == value);
        }

        public static bool operator !=(object value, JiraUser user)
        {
            return !(value == user);
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(DisplayName))
                return DisplayName;
            if (!string.IsNullOrEmpty(Name))
                return Name;
            return Key;
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