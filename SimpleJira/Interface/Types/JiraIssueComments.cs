namespace SimpleJira.Interface.Types
{
    public class JiraIssueComments
    {
        public JiraComment[] Comments { get; set; }
        public int MaxResults { get; set; }
        public int StartAt { get; set; }
        public int Total { get; set; }
    }
}