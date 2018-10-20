using System;

namespace SimpleJira.Interface.ObjectModel
{
    public class JiraComment
    {
        public JiraCommentAuthor Author { get; set; }
        public JiraCommentAuthor UpdateAuthor { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}