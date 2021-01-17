namespace SimpleJira.Interface.Issue
{
    public class JiraCustomFieldValue
    {
        private readonly IJiraIssueFieldsController controller;
        private readonly string id;

        public JiraCustomFieldValue(IJiraIssueFieldsController controller, string id)
        {
            this.controller = controller;
            this.id = id;
        }

        public TValue Get<TValue>()
        {
            return controller.GetValue<TValue>("customfield_" + id);
        }

        public void Set<TValue>(TValue value)
        {
            controller.SetValue("customfield_" + id, value);
        }
    }
}