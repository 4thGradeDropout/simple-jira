using System;

namespace SimpleJira.Interface.Metadata
{
    public class JiraIssuePropertyAttribute : Attribute
    {
        public JiraIssuePropertyAttribute(int customerFieldId)
        {
            FieldName = $"customfield_{customerFieldId}";
            JqlAlias = $"cf[{customerFieldId}]";
        }

        public JiraIssuePropertyAttribute(string fieldName)
        {
            FieldName = fieldName;
            JqlAlias = fieldName;
        }

        public string FieldName { get; }
        public string JqlAlias { get; }
    }
}