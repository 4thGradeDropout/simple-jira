using System;

namespace SimpleJira.Interface.Types
{
    public class JiraProject : IJqlToken
    {
        public JiraAvatarUrls AvatarUrls { get; set; }
        public string Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string ProjectTypeKey { get; set; }
        public string Self { get; set; }

        public override bool Equals(object obj)
        {
            return this == obj;
        }

        public override int GetHashCode()
        {
            return Key == null ? 0 : Key.ToLower().GetHashCode();
        }

        public static bool operator ==(JiraProject project1, JiraProject project2)
        {
            if (ReferenceEquals(project1, null) && ReferenceEquals(project2, null))
                return true;
            if (ReferenceEquals(project1, null) || ReferenceEquals(project2, null))
                return false;
            return project1 == project2.Key || project1 == project2.Id || project1 == project2.Name;
        }

        public static bool operator ==(JiraProject project, string value)
        {
            if (ReferenceEquals(project, null) && value == null)
                return true;
            if (ReferenceEquals(project, null) || value == null)
                return false;
            return string.Equals(project.Key, value, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(project.Id, value, StringComparison.InvariantCultureIgnoreCase)
                   || string.Equals(project.Name, value, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool operator ==(JiraProject project, object value)
        {
            if (value is string stringValue)
                return project == stringValue;
            if (value is JiraProject jiraProject)
                return project == jiraProject;
            return ReferenceEquals(project, value);
        }

        public static bool operator ==(string value, JiraProject project)
        {
            return project == value;
        }

        public static bool operator ==(object value, JiraProject project)
        {
            return project == value;
        }

        public static bool operator !=(JiraProject project1, JiraProject project2)
        {
            return !(project1 == project2);
        }

        public static bool operator !=(JiraProject project, string value)
        {
            return !(project == value);
        }

        public static bool operator !=(string value, JiraProject project)
        {
            return !(value == project);
        }

        public static bool operator !=(JiraProject project, object value)
        {
            return !(project == value);
        }

        public static bool operator !=(object value, JiraProject project)
        {
            return !(value == project);
        }

        public override string ToString()
        {
            return Name;
        }

        public string ToJqlToken()
        {
            if (!string.IsNullOrEmpty(Key))
                return Key;
            if (!string.IsNullOrEmpty(Id))
                return Id;
            if (!string.IsNullOrEmpty(Name))
                return Name;
            return null;
        }
    }
}