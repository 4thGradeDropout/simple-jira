using System.Collections.Generic;
using SimpleJira.Interface.ObjectModel;

namespace SimpleJira.Impl.Mock
{
    internal interface IJiraIssueMockStore
    {
        string KeyPrefix { get; }
        IEnumerable<JiraIssue> Select();
        void Create(JiraIssue issue);
        void Update(JiraIssue issue);
        IEnumerable<JiraComment> SelectComments(string issueKey);
        void AddComment(string issueKey, JiraComment comment);
    }
}