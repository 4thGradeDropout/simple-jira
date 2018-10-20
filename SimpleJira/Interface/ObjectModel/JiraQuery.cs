namespace SimpleJira.Interface.ObjectModel
{
    public class JiraQuery
    {
        public string Expand { get; set; }
        public string Jql { get; set; }
        public int? MaxResults { get; set; }
        public int? StartAt { get; set; }
        public string[] Fields { get; set; }
    }
}