using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleJira.Interface.Types
{
    public class JiraCustomFieldOption : IJqlToken
    {
        public JiraCustomFieldOption Child { get; set; }
        public string Self { get; set; }
        public string Value { get; set; }
        public string Id { get; set; }

        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public override int GetHashCode()
        {
            return Id == null ? 0 : Id.ToLower().GetHashCode();
        }

        public static bool operator ==(JiraCustomFieldOption option, JiraCustomFieldOption[] options)
        {
            if (ReferenceEquals(option, null) && ReferenceEquals(options, null))
                return true;
            if (ReferenceEquals(option, null) || ReferenceEquals(options, null))
                return false;
            return options.Any(x => x == option);
        }

        public static bool operator ==(JiraCustomFieldOption option1, JiraCustomFieldOption option2)
        {
            if (ReferenceEquals(option1, null) && ReferenceEquals(option2, null))
                return true;
            if (ReferenceEquals(option1, null) || ReferenceEquals(option2, null))
                return false;
            return option1 == option2.Id || option1 == option2.Value;
        }

        public static bool operator ==(JiraCustomFieldOption option, string value)
        {
            if (ReferenceEquals(option, null) && value == null)
                return true;
            if (ReferenceEquals(option, null) || value == null)
                return false;
            return string.Equals(option.Id, value, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(option.Value, value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool operator ==(JiraCustomFieldOption option, object value)
        {
            if (value is string stringValue)
                return option == stringValue;
            if (value is JiraCustomFieldOption jiraOption)
                return option == jiraOption;
            if (value is JiraCustomFieldOption[] jiraOptions)
                return option == jiraOptions;
            return ReferenceEquals(option, value);
        }

        public static bool operator ==(JiraCustomFieldOption[] options, JiraCustomFieldOption option)
        {
            return option == options;
        }


        public static bool operator ==(string value, JiraCustomFieldOption status)
        {
            return status == value;
        }


        public static bool operator ==(object value, JiraCustomFieldOption status)
        {
            return status == value;
        }

        public static bool operator !=(JiraCustomFieldOption option, JiraCustomFieldOption[] options)
        {
            return !(option == options);
        }

        public static bool operator !=(JiraCustomFieldOption option1, JiraCustomFieldOption option2)
        {
            return !(option1 == option2);
        }

        public static bool operator !=(JiraCustomFieldOption status, string value)
        {
            return !(status == value);
        }

        public static bool operator !=(JiraCustomFieldOption[] options, JiraCustomFieldOption option)
        {
            return !(options == option);
        }


        public static bool operator !=(string value, JiraCustomFieldOption status)
        {
            return !(value == status);
        }

        public static bool operator !=(JiraCustomFieldOption status, object value)
        {
            return !(status == value);
        }

        public static bool operator !=(object value, JiraCustomFieldOption status)
        {
            return !(value == status);
        }

        public override string ToString()
        {
            var result = new List<string> {Value ?? ""};
            if (!ReferenceEquals(Child, null))
                result.Add(Child.ToString());
            return string.Join(" - ", result);
        }

        public string ToJqlToken()
        {
            if (Child != (JiraCustomFieldOption) null)
                return Child.ToJqlToken();
            if (!string.IsNullOrEmpty(Id))
                return Id;
            if (!string.IsNullOrEmpty(Value))
                return Value;
            return null;
        }
    }
}