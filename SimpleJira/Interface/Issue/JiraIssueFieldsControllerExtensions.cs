namespace SimpleJira.Interface.Issue
{
    public static class JiraIssueFieldsControllerExtensions
    {
        public static TProperty GetValue<TProperty>(this IJiraIssueFieldsController controller, string key)
        {
            return (TProperty) controller.GetValue(key, typeof(TProperty));
        }
    }
}