namespace SimpleJira.Interface.Issue
{
    public interface IConstraintFieldBuilder<TIssue, TField> where TIssue : JiraIssue
    {
        void Value(TField field);
    }
}