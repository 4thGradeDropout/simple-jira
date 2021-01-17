namespace SimpleJira.Interface.Issue
{
    public interface IDefineWorkflow<in TIssue> where TIssue : JiraIssue
    {
        void Build(IWorkflowBuilder<TIssue> builder);
    }
}