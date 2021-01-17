using SimpleJira.Interface.Issue;

namespace SimpleJira.Interface
{
    public class JiraIssuesResponse : JiraIssuesResponse<JiraIssue>
    {
    }

    public class JiraIssuesResponse<TIssue> where TIssue : JiraIssue
    {
        public string Expand { get; set; }
        public TIssue[] Issues { get; set; }
        public int MaxResults { get; set; }
        public int StartAt { get; set; }
        public int Total { get; set; }
    }
}