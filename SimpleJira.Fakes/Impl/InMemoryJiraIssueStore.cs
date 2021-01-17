using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Fakes.Impl
{
    internal class InMemoryJiraIssueStore : IJiraIssueStore
    {
        private readonly List<JiraIssueDto> issues = new List<JiraIssueDto>();

        private readonly ConcurrentDictionary<string, JiraAttachment[]> issueAttachments =
            new ConcurrentDictionary<string, JiraAttachment[]>(StringComparer.InvariantCultureIgnoreCase);

        private readonly ConcurrentDictionary<string, JiraComment[]> issueComments =
            new ConcurrentDictionary<string, JiraComment[]>(StringComparer.InvariantCultureIgnoreCase);

        private readonly ConcurrentDictionary<long, byte[]> attachmentContents =
            new ConcurrentDictionary<long, byte[]>();

        private long id = 754318;
        private readonly object lockObject = new object();

        public Task<long> GenerateId()
        {
            var source = new TaskCompletionSource<long>();
            try
            {
                source.SetResult(Interlocked.Increment(ref id));
            }
            catch (Exception e)
            {
                source.SetException(e);
            }

            return source.Task;
        }

        public async Task<JiraIssueDto> Get(string keyOrId)
        {
            var allIssues = await GetAll();
            var issue = allIssues.SingleOrDefault(x =>
                string.Equals(keyOrId, x.Key, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(keyOrId, x.Id, StringComparison.InvariantCultureIgnoreCase));
            if (issue == null)
                throw new JiraException($"issue '{keyOrId}' is not found");
            return issue;
        }

        public Task<JiraIssueDto[]> GetAll()
        {
            var source = new TaskCompletionSource<JiraIssueDto[]>();
            try
            {
                lock (lockObject)
                {
                    var result = issues.Select(Clone).ToArray();
                    foreach (var issue in result)
                    {
                        if (!issueAttachments.TryGetValue(issue.Key, out var a))
                            a = new JiraAttachment[0];
                        issue.IssueFields.SetProperty("attachment", a.Select(x => new JiraAttachment
                        {
                            Author = x.Author,
                            Content = x.Content,
                            Created = x.Created,
                            Filename = x.Filename,
                            Id = x.Id,
                            Self = x.Self,
                            Size = x.Size
                        }).ToArray());
                        if (!issueComments.TryGetValue(issue.Key, out var c))
                            c = new JiraComment[0];
                        issue.IssueFields.SetProperty("comment", new JiraIssueComments
                        {
                            Comments = c.Select(x => new JiraComment
                            {
                                Author = x.Author,
                                Body = x.Body,
                                Created = x.Created,
                                Id = x.Id,
                                Self = x.Self,
                                Updated = x.Updated,
                                UpdateAuthor = x.UpdateAuthor
                            }).ToArray(),
                            StartAt = 0,
                            Total = c.Length,
                            MaxResults = c.Length
                        });
                    }

                    source.SetResult(result);
                }
            }
            catch (Exception e)
            {
                source.SetException(e);
            }

            return source.Task;
        }

        public Task Insert(JiraIssueDto issue)
        {
            var source = new TaskCompletionSource<bool>();
            try
            {
                lock (lockObject) issues.Add(Clone(issue));

                source.SetResult(true);
            }
            catch (Exception e)
            {
                source.SetException(e);
            }

            return source.Task;
        }

        public Task Update(string keyOrId, JiraIssueFields fields)
        {
            var source = new TaskCompletionSource<bool>();
            try
            {
                lock (lockObject)
                {
                    var stored = issues.SingleOrDefault(x =>
                        string.Equals(x.Key, keyOrId, StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(x.Id, keyOrId, StringComparison.InvariantCultureIgnoreCase));
                    if (stored == null)
                        throw new JiraException($"issue '{keyOrId}' is not found");
                    fields.CopyTo(stored.IssueFields);
                    source.SetResult(true);
                }
            }
            catch (Exception e)
            {
                source.SetException(e);
            }

            return source.Task;
        }

        public Task UploadAttachment(string keyOrId, long attachmentId, JiraAttachment attachment, byte[] content)
        {
            var source = new TaskCompletionSource<bool>();
            try
            {
                lock (lockObject)
                {
                    var issue = issues.SingleOrDefault(x =>
                        string.Equals(x.Key, keyOrId, StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(x.Id, keyOrId, StringComparison.InvariantCultureIgnoreCase));
                    if (issue == null)
                        throw new JiraException($"issue '{keyOrId}' is not found");
                    var bytes = new byte[content.Length];
                    Buffer.BlockCopy(content, 0, bytes, 0, content.Length);
                    attachmentContents.TryAdd(attachmentId, bytes);
                    var clonedAttachment = new JiraAttachment
                    {
                        Author = attachment.Author,
                        Content = attachment.Content,
                        Created = attachment.Created,
                        Filename = attachment.Filename,
                        Id = attachment.Id,
                        Self = attachment.Self,
                        Size = attachment.Size
                    };

                    issueAttachments.AddOrUpdate(issue.Key, key => new[] {clonedAttachment},
                        (key, attachments) => attachments.Append(clonedAttachment).ToArray());
                    source.SetResult(true);
                }
            }
            catch (Exception e)
            {
                source.SetException(e);
            }

            return source.Task;
        }

        public Task<byte[]> DownloadAttachment(long attachmentId)
        {
            var source = new TaskCompletionSource<byte[]>();
            try
            {
                if (!attachmentContents.TryGetValue(attachmentId, out var content))
                    throw new JiraException($"attachment '{attachmentId}' is not found");
                var bytes = new byte[content.Length];
                Buffer.BlockCopy(content, 0, bytes, 0, content.Length);
                source.SetResult(bytes);
            }
            catch (Exception e)
            {
                source.SetException(e);
            }

            return source.Task;
        }

        public Task DeleteAttachment(long attachmentId)
        {
            var source = new TaskCompletionSource<bool>();
            try
            {
                lock (lockObject)
                {
                    var issueKey = issueAttachments.Where(x => x.Value.Any(y => y.Id == attachmentId.ToString()))
                        .Select(x => x.Key)
                        .SingleOrDefault();
                    if (issueKey == null)
                        throw new JiraException($"attachment '{attachmentId}' is nt found");
                    attachmentContents.TryRemove(attachmentId, out _);
                    issueAttachments.AddOrUpdate(issueKey,
                        key => new JiraAttachment[0],
                        (key, attachments) => attachments.Where(x => x.Id != attachmentId.ToString()).ToArray());
                    source.SetResult(true);
                }
            }
            catch (Exception e)
            {
                source.SetException(e);
            }

            return source.Task;
        }


        public Task<JiraComment[]> GetComments(string keyOrId)
        {
            var source = new TaskCompletionSource<JiraComment[]>();
            try
            {
                lock (lockObject)
                {
                    var issue = issues.SingleOrDefault(x =>
                        string.Equals(x.Key, keyOrId, StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(x.Id, keyOrId, StringComparison.InvariantCultureIgnoreCase));
                    if (issue == null)
                        throw new JiraException($"issue '{keyOrId}' is not found");
                    if (!issueComments.TryGetValue(issue.Key, out var comments))
                        comments = new JiraComment[0];
                    source.SetResult(comments);
                }
            }
            catch (Exception e)
            {
                source.SetException(e);
            }

            return source.Task;
        }

        public Task AddComment(string keyOrId, JiraComment comment)
        {
            var source = new TaskCompletionSource<bool>();
            try
            {
                lock (lockObject)
                {
                    var issue = issues.SingleOrDefault(x =>
                        string.Equals(x.Key, keyOrId, StringComparison.InvariantCultureIgnoreCase)
                        || string.Equals(x.Id, keyOrId, StringComparison.InvariantCultureIgnoreCase));
                    if (issue == null)
                        throw new JiraException($"issue '{keyOrId}' is not found");
                    var clonedComment = new JiraComment
                    {
                        Author = comment.Author,
                        Body = comment.Body,
                        Created = comment.Created,
                        Id = comment.Id,
                        Self = comment.Self,
                        Updated = comment.Updated,
                        UpdateAuthor = comment.UpdateAuthor
                    };

                    issueComments.AddOrUpdate(issue.Key, key => new[] {clonedComment},
                        (key, comments) => comments.Append(clonedComment).ToArray());
                    source.SetResult(true);
                }
            }
            catch (Exception e)
            {
                source.SetException(e);
            }

            return source.Task;
        }

        public void Drop()
        {
            lock (lockObject)
            {
                issues.Clear();
                issueAttachments.Clear();
                attachmentContents.Clear();
                issueComments.Clear();
            }
        }

        private static JiraIssueDto Clone(JiraIssueDto issue)
        {
            return new JiraIssueDto
            {
                Id = issue.Id,
                Key = issue.Key,
                Self = issue.Self,
                IssueFields = issue.IssueFields.Clone()
            };
        }
    }
}