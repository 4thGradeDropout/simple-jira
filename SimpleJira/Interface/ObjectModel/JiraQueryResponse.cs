namespace SimpleJira.Interface.ObjectModel
{
    public class JiraQueryResponse
    {
        public string Expand { get; set; }
        public JiraIssue[] Issues { get; set; }
        public int MaxResults { get; set; }
        public int StartAt { get; set; }
        public int Total { get; set; }
    }
}