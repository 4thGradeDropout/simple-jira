using System;
using System.Linq;
using SimpleJira.Interface.Helpers;
using SimpleJira.Interface.ObjectModel;

namespace SimpleJira.Impl.Mock.Jql
{
    internal static class JqlFieldEqualityComparer
    {
        public static bool Equals(JiraMetadata metadata, JiraIssue issue, string field, string value)
        {
            if (EqualsIgnoreCase(field, "key"))
                return EqualsIgnoreCase(issue.Key, value);
            if (EqualsIgnoreCase(field, "project"))
            {
                if (value == null)
                    return issue.Path<object>("project") == null;
                var project = DetectProject(metadata, value);
                if (project == null)
                    return false;
                var id = issue.Path<string>("project.id");
                var key = issue.Path<string>("project.key");
                var name = issue.Path<string>("project.name");
                return EqualsIgnoreCase(project.Id, id)
                       || EqualsIgnoreCase(project.Key, key)
                       || EqualsIgnoreCase(project.Name, name);
            }
            if (EqualsIgnoreCase(field, "status"))
            {
                var issueStatusName = issue.Path<object>("status.name") as string;
                var expectedStatusName = DecodeStatusName(value);
                return EqualsIgnoreCase(issueStatusName, expectedStatusName);
            }
            var primitiveValue = issue.Path<object>(field) as string;
            var objectValue = issue.Path<object>(field + ".value") as string;
            var objectId = issue.Path<object>(field + ".id") as string;
            return EqualsIgnoreCase(primitiveValue, value)
                   || EqualsIgnoreCase(objectValue, value)
                   || EqualsIgnoreCase(objectId, value);
        }

        private static string DecodeStatusName(string value)
        {
            if (EqualsIgnoreCase(value, "Open"))
                return "Открыта";
            if (EqualsIgnoreCase(value, "In Progress"))
                return "В работе";
            if (EqualsIgnoreCase(value, "Reopened"))
                return "Возобновлена";
            if (EqualsIgnoreCase(value, "Closed"))
                return "Проверена";
            return value;
        }

        private static JiraProject DetectProject(JiraMetadata metadata, string description)
        {
            var projects = metadata.GetProjects();
            var matchedProjects = projects.Where(x =>
                    EqualsIgnoreCase(x.Id, description) || EqualsIgnoreCase(x.Key, description) ||
                    EqualsIgnoreCase(x.Name, description))
                .Take(2)
                .ToArray();
            switch (matchedProjects.Length)
            {
                case 0:
                    return null;
                case 1:
                    return matchedProjects[0];
                default:
                    throw new InvalidOperationException(
                        "more than one same projects with the same requisites detected: " +
                        $"(id: [{matchedProjects[0].Id}], key=[{matchedProjects[0].Key}], name=[{matchedProjects[0].Name}]) and " +
                        $"(id: [{matchedProjects[1].Id}], key=[{matchedProjects[1].Key}], name=[{matchedProjects[1].Name}])");
            }
        }

        private static bool EqualsIgnoreCase(string s1, string s2)
        {
            return string.Equals(s1, s2, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}