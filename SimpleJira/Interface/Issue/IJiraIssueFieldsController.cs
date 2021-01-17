using System;

namespace SimpleJira.Interface.Issue
{
    public interface IJiraIssueFieldsController
    {
        void SetValue(string key, object value);
        object GetValue(string key, Type type);
        JiraIssueFields GetFields();
        JiraIssueFields GetChangedFields();
        void RegisterUnchangingField(string fieldName);
    }
}