using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SimpleJira.Fakes.Impl.Helpers;
using SimpleJira.Impl.Serialization;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Fakes.Impl
{
    internal class FileJiraIssueStore : IJiraIssueStore
    {
        private readonly string folderPath;
        private readonly string mutexName;

        private readonly SequenceGenerator sequenceGenerator;

        private readonly BlobStorage issueStorage;
        private readonly BlobStorage attachmentsHeaders;
        private readonly BlobStorage attachmentsContents;
        private readonly BlobStorage commentsStorage;

        private readonly BlobStorage issueIdsIndex;
        private readonly BlobStorage attachmentsIssueIndex;
        private readonly BlobStorage issueAttachmentsIndex;
        private readonly BlobStorage issueCommentsIndex;

        private static readonly JiraAttachment[] emptyAttachments = new JiraAttachment[0];
        private static readonly JiraComment[] emptyComments = new JiraComment[0];

        public FileJiraIssueStore(string folderPath)
        {
            this.folderPath = folderPath;
            sequenceGenerator = new SequenceGenerator(folderPath, "sequence");
            issueStorage = new BlobStorage(folderPath, "issues", "issues.index");
            attachmentsContents = new BlobStorage(folderPath, "attachments.content", "attachments.content.index");
            attachmentsHeaders = new BlobStorage(folderPath, "attachments.header", "attachments.header.index");
            commentsStorage = new BlobStorage(folderPath, "comments", "comments.index");

            issueIdsIndex = new BlobStorage(folderPath, "issues.ids", "issues.ids.index");
            issueCommentsIndex = new BlobStorage(folderPath, "issues.comment", "issues.comment.index");
            issueAttachmentsIndex = new BlobStorage(folderPath, "issues.attachment", "issues.attachment.index");
            attachmentsIssueIndex = new BlobStorage(folderPath, "attachments.issue", "attachments.issue.index");

            mutexName = "Global\\" + folderPath.Replace(":", "_")
                            .Replace("/", "_")
                            .Replace("\\", "_");
        }

        public Task<long> GenerateId()
        {
            return InvokeByLock(() => sequenceGenerator.NextValue() + 754318);
        }

        public Task<JiraIssueDto> Get(string keyOrId)
        {
            return InvokeByLock(() =>
            {
                var key = GetIssueStorageKey(keyOrId);
                if (!issueStorage.TryRead(key, out var bytes))
                    throw new JiraException($"issue '{keyOrId}' is not found");
                var result = JiraIssueDto.FromBytes(bytes);
                if (issueAttachmentsIndex.TryRead(key, out var attachmentBytes))
                {
                    var attachmentIds = LongArrayFromBytes(attachmentBytes);
                    var issuesAttachments = new JiraAttachment[attachmentIds.Length];
                    for (var i = 0; i < issuesAttachments.Length; ++i)
                    {
                        if (!attachmentsHeaders.TryRead(attachmentIds[i], out var headerBytes))
                            throw new InvalidOperationException(
                                $"attachment '{attachmentIds[i]}' is not found");
                        issuesAttachments[i] =
                            Json.Deserialize<JiraAttachment>(Encoding.UTF8.GetString(headerBytes));
                    }

                    result.IssueFields.SetProperty("attachment", issuesAttachments);
                }
                else
                    result.IssueFields.SetProperty("attachment", new JiraAttachment[0]);

                var issueComments = ReadIssueComments(key);
                result.IssueFields.SetProperty("comment", new JiraIssueComments
                {
                    Comments = issueComments,
                    StartAt = 0,
                    Total = issueComments.Length,
                    MaxResults = issueComments.Length
                });
                return result;
            });
        }

        public Task<JiraIssueDto[]> GetAll()
        {
            return InvokeByLock(() =>
            {
                var allAttachments = new Dictionary<long, JiraAttachment>();
                foreach (var pair in attachmentsHeaders.ReadAll())
                    allAttachments[pair.Key] =
                        Json.Deserialize<JiraAttachment>(Encoding.UTF8.GetString(pair.Value));
                var attachmentsIndex = new Dictionary<long, JiraAttachment[]>();
                foreach (var pair in issueAttachmentsIndex.ReadAll())
                {
                    var attachmentIds = LongArrayFromBytes(pair.Value);
                    var issueAttachments = new JiraAttachment[attachmentIds.Length];
                    for (var i = 0; i < attachmentIds.Length; ++i)
                        issueAttachments[i] = allAttachments[attachmentIds[i]];
                    attachmentsIndex[pair.Key] = issueAttachments;
                }

                var allComments = new Dictionary<long, JiraComment>();
                foreach (var pair in commentsStorage.ReadAll())
                    allComments[pair.Key] = Json.Deserialize<JiraComment>(Encoding.UTF8.GetString(pair.Value));

                var commentsIndex = new Dictionary<long, JiraComment[]>();
                foreach (var pair in issueCommentsIndex.ReadAll())
                {
                    var commentIds = LongArrayFromBytes(pair.Value);
                    var issueComments = new JiraComment[commentIds.Length];
                    for (var i = 0; i < commentIds.Length; ++i)
                        issueComments[i] = allComments[commentIds[i]];
                    commentsIndex[pair.Key] = issueComments;
                }

                var issues = new List<JiraIssueDto>();
                foreach (var pair in issueStorage.ReadAll())
                {
                    var jiraIssueDto = JiraIssueDto.FromBytes(pair.Value);
                    var keyId = ExtractLong(jiraIssueDto.Key);
                    if (attachmentsIndex.TryGetValue(keyId, out var a))
                        jiraIssueDto.IssueFields.SetProperty("attachment", a);
                    else
                        jiraIssueDto.IssueFields.SetProperty("attachment", emptyAttachments);
                    if (!commentsIndex.TryGetValue(keyId, out var c))
                        c = emptyComments;
                    jiraIssueDto.IssueFields.SetProperty("comment", new JiraIssueComments
                    {
                        Comments = c,
                        StartAt = 0,
                        Total = c.Length,
                        MaxResults = c.Length
                    });
                    issues.Add(jiraIssueDto);
                }

                return issues.ToArray();
            });
        }

        public Task Insert(JiraIssueDto jObject)
        {
            return InvokeByLock(() =>
            {
                var key = ExtractLong(jObject.Key);
                var id = long.Parse(jObject.Id);
                issueStorage.Write(key, jObject.ToBytes());
                issueIdsIndex.Write(id, BitConverter.GetBytes(key));
                return true;
            });
        }

        public Task Update(string keyOrId, JiraIssueFields fields)
        {
            return InvokeByLock(() =>
            {
                var key = GetIssueStorageKey(keyOrId);
                if (!issueStorage.TryRead(key, out var issueBytes))
                    throw new JiraException($"issue [{keyOrId}] is not found");
                var replacedIssue = JiraIssueDto.FromBytes(issueBytes);
                fields.CopyTo(replacedIssue.IssueFields);
                issueStorage.Write(key, replacedIssue.ToBytes());
                return true;
            });
        }

        public Task UploadAttachment(string keyOrId, long attachmentId, JiraAttachment attachment, byte[] content)
        {
            return InvokeByLock(() =>
            {
                var key = GetIssueStorageKey(keyOrId);
                var bytes = Encoding.UTF8.GetBytes(Json.Serialize(attachment));
                attachmentsHeaders.Write(attachmentId, bytes);
                attachmentsContents.Write(attachmentId, content);
                attachmentsIssueIndex.Write(attachmentId, BitConverter.GetBytes(key));
                byte[] newAttachmentIdsBytes;
                int currentAttachmentOffset;
                if (issueAttachmentsIndex.TryRead(key, out var attachmentIdsBytes))
                {
                    currentAttachmentOffset = attachmentIdsBytes.Length;
                    newAttachmentIdsBytes = new byte[currentAttachmentOffset + sizeof(long)];
                    Buffer.BlockCopy(attachmentIdsBytes, 0, newAttachmentIdsBytes, 0, currentAttachmentOffset);
                }
                else
                {
                    currentAttachmentOffset = 0;
                    newAttachmentIdsBytes = new byte[sizeof(long)];
                }

                Buffer.BlockCopy(BitConverter.GetBytes(attachmentId), 0, newAttachmentIdsBytes,
                    currentAttachmentOffset, sizeof(long));
                issueAttachmentsIndex.Write(key, newAttachmentIdsBytes);
                return true;
            });
        }

        public Task<byte[]> DownloadAttachment(long attachmentId)
        {
            return InvokeByLock(() =>
            {
                if (!attachmentsContents.TryRead(attachmentId, out var bytes))
                    throw new JiraException($"attachment [{attachmentId}] is not found");
                return bytes;
            });
        }

        public Task DeleteAttachment(long attachmentId)
        {
            return InvokeByLock(() =>
            {
                if (!attachmentsIssueIndex.TryRead(attachmentId, out byte[] keyBytes))
                    throw new JiraException($"attachment '{attachmentId}' is not found");
                var key = BitConverter.ToInt64(keyBytes, 0);
                attachmentsHeaders.TryDelete(attachmentId);
                attachmentsContents.TryDelete(attachmentId);
                attachmentsIssueIndex.TryDelete(attachmentId);
                if (issueAttachmentsIndex.TryRead(key, out var attachmentIdsBytes))
                {
                    var readOffset = 0;
                    var writeOffset = 0;
                    var ignoredBytes = 0;
                    var newAttachmentIdsBytes = new byte[attachmentIdsBytes.Length];
                    while (readOffset < attachmentIdsBytes.Length)
                    {
                        var currentAttachmentId = BitConverter.ToInt64(attachmentIdsBytes, readOffset);
                        if (currentAttachmentId == attachmentId)
                            ignoredBytes += sizeof(long);
                        else
                        {
                            Buffer.BlockCopy(attachmentIdsBytes, readOffset, newAttachmentIdsBytes, writeOffset,
                                sizeof(long));
                            writeOffset += sizeof(long);
                        }

                        readOffset += sizeof(long);
                    }

                    if (ignoredBytes > 0)
                        Array.Resize(ref newAttachmentIdsBytes, attachmentIdsBytes.Length - ignoredBytes);
                    issueAttachmentsIndex.Write(key, newAttachmentIdsBytes);
                }

                return true;
            });
        }

        public Task<JiraComment[]> GetComments(string keyOrId)
        {
            return InvokeByLock(() =>
            {
                var key = GetIssueStorageKey(keyOrId);
                return ReadIssueComments(key);
            });
        }

        public Task AddComment(string keyOrId, JiraComment comment)
        {
            return InvokeByLock(() =>
            {
                var key = GetIssueStorageKey(keyOrId);
                var commentId = long.Parse(comment.Id);
                commentsStorage.Write(commentId, Encoding.UTF8.GetBytes(Json.Serialize(comment)));
                byte[] newCommentIdsBytes;
                int currentCommentOffset;
                if (issueCommentsIndex.TryRead(key, out var commentsIdsBytes))
                {
                    currentCommentOffset = commentsIdsBytes.Length;
                    newCommentIdsBytes = new byte[currentCommentOffset + sizeof(long)];
                    Buffer.BlockCopy(commentsIdsBytes, 0, newCommentIdsBytes, 0, currentCommentOffset);
                }
                else
                {
                    currentCommentOffset = 0;
                    newCommentIdsBytes = new byte[sizeof(long)];
                }

                Buffer.BlockCopy(BitConverter.GetBytes(commentId), 0, newCommentIdsBytes,
                    currentCommentOffset, sizeof(long));
                issueCommentsIndex.Write(key, newCommentIdsBytes);
                return true;
            });
        }

        public void Drop()
        {
            using (GlobalLock())
            {
                sequenceGenerator.Drop();
                issueStorage.Drop();
                issueIdsIndex.Drop();
                attachmentsContents.Drop();
                attachmentsHeaders.Drop();
                issueAttachmentsIndex.Drop();
                attachmentsIssueIndex.Drop();
                commentsStorage.Drop();
                issueCommentsIndex.Drop();
                if (Directory.Exists(folderPath))
                    Directory.Delete(folderPath, true);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task<T> InvokeByLock<T>(Func<T> invoker)
        {
            var source = new TaskCompletionSource<T>();
            try
            {
                using (GlobalLock())
                {
                    source.SetResult(invoker());
                }
            }
            catch (Exception e)
            {
                source.SetException(e);
            }

            return source.Task;
        }

        private long GetIssueStorageKey(string keyOrId)
        {
            if (string.IsNullOrEmpty(keyOrId))
                throw new ArgumentNullException(keyOrId);
            if (!TryExtractIssueKey(keyOrId, out var key))
            {
                if (!long.TryParse(keyOrId, out var id))
                    throw new InvalidOperationException(
                        $"issue identifier should be issue's key or id, but was '{keyOrId}'");
                if (!issueIdsIndex.TryRead(id, out var keyBytes))
                    throw new JiraException($"issue '{keyOrId}' is not found");
                key = BitConverter.ToInt64(keyBytes, 0);
            }

            return key;
        }

        private JiraComment[] ReadIssueComments(long key)
        {
            if (issueCommentsIndex.TryRead(key, out var commentIdsBytes))
            {
                var commentIds = LongArrayFromBytes(commentIdsBytes);
                var result = new JiraComment[commentIds.Length];
                for (var i = 0; i < commentIds.Length; ++i)
                {
                    if (!commentsStorage.TryRead(commentIds[i], out var commentBytes))
                        throw new JiraException($"comment '{commentIds[i]}' is not found");
                    result[i] = Json.Deserialize<JiraComment>(Encoding.UTF8.GetString(commentBytes));
                }

                return result;
            }

            return new JiraComment[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long[] LongArrayFromBytes(byte[] attachmentIdsBytes)
        {
            var offset = 0;
            var index = 0;
            var issueAttachments = new long[attachmentIdsBytes.Length / sizeof(long)];
            while (offset < attachmentIdsBytes.Length)
            {
                var attachmentId = BitConverter.ToInt64(attachmentIdsBytes, offset);
                issueAttachments[index] = attachmentId;
                ++index;
                offset += sizeof(long);
            }

            return issueAttachments;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IDisposable GlobalLock()
        {
            return MutexLock.Lock(mutexName, TimeSpan.FromSeconds(5));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long ExtractLong(string key)
        {
            if (!TryExtractIssueKey(key, out var result))
                throw new InvalidOperationException($"can't parse long part from issue key '{key}'");
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool TryExtractIssueKey(string key, out long longPart)
        {
            if (string.IsNullOrEmpty(key))
            {
                longPart = default;
                return false;
            }

            var indexOf = key.IndexOf('-');
            if (indexOf < 0)
            {
                longPart = default;
                return false;
            }

            return long.TryParse(key.Substring(indexOf + 1), out longPart);
        }
    }
}