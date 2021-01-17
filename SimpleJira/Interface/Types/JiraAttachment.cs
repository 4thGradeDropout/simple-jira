using System;

namespace SimpleJira.Interface.Types
{
    public class JiraAttachment
    {
        public JiraUser Author { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public string Filename { get; set; }
        public string Id { get; set; }
        public string Self { get; set; }
        public int Size { get; set; }
    }
}