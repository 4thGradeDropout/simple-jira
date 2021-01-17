namespace SimpleJira.Interface
{
    public class JiraIssuesRequest
    {
        public string Jql { get; set; }
        public int StartAt { get; set; }
        public int MaxResults { get; set; }
        public string[] Fields { get; set; }
        public bool ValidateQuery { get; set; }
        public string[] Expand { get; set; }
    }
}