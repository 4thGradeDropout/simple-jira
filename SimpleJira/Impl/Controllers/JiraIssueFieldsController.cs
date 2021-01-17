using System;
using System.Collections.Generic;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Impl.Controllers
{
    internal class JiraIssueFieldsController : IJiraIssueFieldsController
    {
        private readonly JiraIssueFields issueFields;
        private JiraIssueFields changedFields = new JiraIssueFields();
        private readonly HashSet<string> scopeFields = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public JiraIssueFieldsController(JiraIssueFields issueFields)
        {
            this.issueFields = issueFields;
        }

        public void SetValue(string key, object value)
        {
            changedFields.SetProperty(key, value);
        }

        public object GetValue(string key, Type type)
        {
            return changedFields.TryGetProperty(key, type, out var result)
                ? result
                : issueFields.GetProperty(key, type);
        }

        public JiraIssueFields GetFields()
        {
            changedFields.CopyTo(issueFields);
            changedFields = new JiraIssueFields();
            return issueFields;
        }

        public JiraIssueFields GetChangedFields()
        {
            var result = changedFields;
            if (scopeFields.Count > 0)
            {
                result = new JiraIssueFields();
                changedFields.CopyTo(result, scopeFields);
            }

            result.CopyTo(issueFields);
            changedFields = new JiraIssueFields();
            return result;
        }

        public void RegisterUnchangingField(string fieldName)
        {
            scopeFields.Add(fieldName);
        }
    }
}