using SimpleJira.Interface.Types;

namespace SimpleJira.Interface
{
    public class JiraCommentsResponse
    {
        public JiraComment[] Comments { get; set; }
        public int MaxResults { get; set; }
        public int StartAt { get; set; }
        public int Total { get; set; }
    }
}