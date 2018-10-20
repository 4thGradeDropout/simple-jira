using System.Collections.Generic;
using System.Linq;
using SimpleJira.Impl.Utilities;
using SimpleJira.Interface.ObjectModel;

namespace SimpleJira.Impl.Mock.InMemory
{
    internal class InMemoryJiraIssueMockStore : IJiraIssueMockStore
    {
        private readonly List<JiraIssue> issues = new List<JiraIssue>();
        private readonly Dictionary<string, List<JiraComment>> comments = new Dictionary<string, List<JiraComment>>();

        public InMemoryJiraIssueMockStore(string keyPrefix)
        {
            KeyPrefix = keyPrefix;
        }

        public string KeyPrefix { get; }

        public IEnumerable<JiraIssue> Select()
        {
            return issues.Select(CloneIssue);
        }

        public void Create(JiraIssue issue)
        {
            issues.Add(CloneIssue(issue));
        }

        public void Update(JiraIssue issue)
        {
            for (var i = 0; i < issues.Count; ++i)
                if (issues[i].Key == issue.Key)
                {
                    issues[i] = new JiraIssue
                    {
                        Key = issue.Key,
                        Id = issue.Id,
                        Fields = issue.Fields.ToDictionary()
                    };
                    break;
                }
        }

        public IEnumerable<JiraComment> SelectComments(string issueKey)
        {
            return comments.TryGetValue(issueKey, out var c)
                ? c.Select(CloneComment)
                : new JiraComment[0];
        }

        public void AddComment(string issueKey, JiraComment comment)
        {
            if (!comments.TryGetValue(issueKey, out var c))
            {
                c = new List<JiraComment>();
                comments.Add(issueKey, c);
            }
            c.Add(CloneComment(comment));
        }

        private static JiraIssue CloneIssue(JiraIssue x)
        {
            return new JiraIssue
            {
                Fields = x.Fields.ToDictionary(),
                Id = x.Id,
                Key = x.Key
            };
        }

        private static JiraComment CloneComment(JiraComment comment)
        {
            return new JiraComment
            {
                Author = CloneCommentAuthor(comment.Author),
                Body = comment.Body,
                Created = comment.Created,
                UpdateAuthor = CloneCommentAuthor(comment.UpdateAuthor),
                Updated = comment.Updated
            };
        }

        private static JiraCommentAuthor CloneCommentAuthor(JiraCommentAuthor author)
        {
            if (author == null)
                return null;
            return new JiraCommentAuthor
            {
                Active = author.Active,
                DisplayName = author.DisplayName,
                Name = author.Name,
                Self = author.Self
            };
        }
    }
}