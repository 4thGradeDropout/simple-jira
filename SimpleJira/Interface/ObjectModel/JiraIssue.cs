using System.Collections.Generic;

namespace SimpleJira.Interface.ObjectModel
{
    public class JiraIssue
    {
        public string Key { get; set; }
        public string Id { get; set; }
        public Dictionary<string, object> Fields { get; set; }
    }
}