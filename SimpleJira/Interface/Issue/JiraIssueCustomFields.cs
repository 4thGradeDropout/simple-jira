using System.Collections.Concurrent;

namespace SimpleJira.Interface.Issue
{
    public class JiraIssueCustomFields
    {
        private readonly IJiraIssueFieldsController controller;

        private readonly ConcurrentDictionary<string, JiraCustomFieldValue> fields =
            new ConcurrentDictionary<string, JiraCustomFieldValue>();

        public JiraIssueCustomFields(IJiraIssueFieldsController controller)
        {
            this.controller = controller;
        }

        public JiraCustomFieldValue this[int id] => this[id.ToString()];

        public JiraCustomFieldValue this[string id] =>
            fields.GetOrAdd(id, i => new JiraCustomFieldValue(controller, i));
    }
}