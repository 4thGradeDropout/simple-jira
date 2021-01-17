using SimpleJira.Impl.Helpers;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Impl
{
    internal static class JiraIssueInitializer
    {
        public static void Initialize(JiraIssue issue)
        {
            var scope = Scope.Get(issue.GetType());
            scope.Initialize(issue);
            var properties = scope.Properties;
            if (properties == null || properties.Count == 0) return;
            var metadata = new JiraMetadataProvider(new[] {issue.GetType()});
            foreach (var property in properties)
            {
                var field = metadata.GetFieldMetadata(property);
                issue.Controller.RegisterUnchangingField(field.FieldName);
            }
        }
    }
}