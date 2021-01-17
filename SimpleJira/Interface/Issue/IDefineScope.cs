namespace SimpleJira.Interface.Issue
{
    /// <summary>
    /// Interface for declaring issue's fields constraints.
    /// </summary>
    /// <typeparam name="TIssue">Type of the custom implementation <c>SimpleJira.Interface.Types.JiraIssue</c> which should contains fields' constraints.</typeparam>
    public interface IDefineScope<TIssue> where TIssue : JiraIssue
    {
        /// <summary>
        /// Declares issue's fields constraints.
        /// </summary>
        /// <typeparam name="TIssue">Type of the custom implementation <c>SimpleJira.Interface.Types.JiraIssue</c> which should contains fields' constraints.</typeparam>
        void Build(IScopeBuilder<TIssue> builder);
    }
}