using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleJira.Impl.Helpers;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Interface.Metadata
{
    public class JiraMetadataProvider : IJiraMetadataProvider
    {
        private static readonly ConcurrentDictionary<Type, IJiraIssueMetadata> issues =
            new ConcurrentDictionary<Type, IJiraIssueMetadata>();

        public JiraMetadataProvider(IEnumerable<Type> issueTypes)
        {
            var types = issueTypes.Append(typeof(JiraIssue)).Distinct().ToArray();
            Issues = BuildIssues(types);
        }

        private static JiraMetadataCollection BuildIssues(IEnumerable<Type> issueTypes)
        {
            return new JiraMetadataCollection(issueTypes.Select(GetIssue).ToList());
        }

        private static IJiraIssueMetadata GetIssue(Type type)
        {
            return issues.GetOrAdd(type, issueType =>
            {
                var properties = new List<IJiraFieldMetadata>();
                var workflow = GetWorkflow(issueType);
                var propertyInfos =
                    issueType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var propertyInfo in propertyInfos)
                {
                    var attribute = propertyInfo.GetCustomAttribute<JiraIssuePropertyAttribute>();
                    if (attribute != null)
                    {
                        var propertyMetadata = new JiraFieldMetadata(attribute.FieldName, propertyInfo);
                        properties.Add(propertyMetadata);
                    }
                }

                if (issueType == typeof(JiraIssue))
                {
                    var props = typeof(JiraIssue).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    properties.Add(new JiraFieldMetadata("key", props.Single(x => x.Name == "Key")));
                    properties.Add(new JiraFieldMetadata("id", props.Single(x => x.Name == "Id")));
                    properties.Add(new JiraFieldMetadata("self", props.Single(x => x.Name == "Self")));
                    var statusCategoryProperty = typeof(JiraStatus).GetProperty("StatusCategory",
                        BindingFlags.Public | BindingFlags.Instance);
                    properties.Add(new JiraFieldMetadata("statusCategory", statusCategoryProperty));
                }

                return new JiraIssueMetadata(issueType, new JiraFieldMetadataCollection(properties), workflow);
            });
        }

        private static object GetWorkflow(Type issueType)
        {
            var iDefineWorkflowType = typeof(IDefineWorkflow<>).MakeGenericType(issueType);
            var nestedTypes = issueType.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
            var workflowType = nestedTypes.SingleOrDefault(x => iDefineWorkflowType.IsAssignableFrom(x));
            return workflowType != null ? workflowType.New() : null;
        }

        public IJiraIssueMetadataCollection Issues { get; }

        private class JiraMetadataCollection : IJiraIssueMetadataCollection
        {
            private readonly IEnumerable<IJiraIssueMetadata> issues;

            public JiraMetadataCollection(IEnumerable<IJiraIssueMetadata> issues)
            {
                this.issues = issues;
            }

            public IEnumerator<IJiraIssueMetadata> GetEnumerator()
            {
                return issues.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class JiraIssueMetadata : IJiraIssueMetadata
        {
            public JiraIssueMetadata(Type type, IJiraFieldMetadataCollection fields, object workflow)
            {
                Type = type;
                Fields = fields;
                Workflow = workflow;
            }

            public Type Type { get; }
            public object Workflow { get; }
            public IJiraFieldMetadataCollection Fields { get; }
        }

        private class JiraFieldMetadataCollection : IJiraFieldMetadataCollection
        {
            private readonly IEnumerable<IJiraFieldMetadata> properties;

            public JiraFieldMetadataCollection(IEnumerable<IJiraFieldMetadata> properties)
            {
                this.properties = properties;
            }

            public IEnumerator<IJiraFieldMetadata> GetEnumerator()
            {
                return properties.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private class JiraFieldMetadata : IJiraFieldMetadata
        {
            public JiraFieldMetadata(string fieldName, PropertyInfo property)
            {
                FieldName = fieldName;
                Property = property;
            }

            public string FieldName { get; }
            public PropertyInfo Property { get; }
        }
    }
}