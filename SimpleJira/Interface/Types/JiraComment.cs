using System;

namespace SimpleJira.Interface.Types
{
    public class JiraComment
    {
        public JiraUser Author { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; set; }
        public string Id { get; set; }
        public string Self { get; set; }
        public JiraUser UpdateAuthor { get; set; }
        public DateTime Updated { get; set; }
    }
}