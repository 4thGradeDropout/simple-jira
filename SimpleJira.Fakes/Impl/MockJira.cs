using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SimpleJira.Fakes.Impl.Jql.Compiler;
using SimpleJira.Fakes.Interface;
using SimpleJira.Impl.Controllers;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;
using SimpleJira.Interface.Types;

namespace SimpleJira.Fakes.Impl
{
    internal class MockJira : IMockJira
    {
        private const int maxPacketSize = 200;
        private readonly IJiraIssueStore store;
        private readonly string fakeJiraUrl;
        private readonly JiraUser authorizedUser;
        private readonly IJiraMetadataProvider metadataProvider;

        public MockJira(string fakeJiraUrl, JiraUser authorizedUser,
            IJiraIssueStore store, IJiraMetadataProvider metadataProvider)
        {
            this.store = store;
            this.fakeJiraUrl = fakeJiraUrl;
            this.authorizedUser = authorizedUser;
            this.metadataProvider = metadataProvider;
        }

        public async Task<JiraIssuesResponse> SelectIssuesAsync(JiraIssuesRequest request,
            CancellationToken cancellationToken)
        {
            var jqlCommand = JqlCompiler.Compile(request.Jql, metadataProvider);
            var issues = jqlCommand.Execute(await store.GetAll());
            var maxResults = request.MaxResults == 0
                ? maxPacketSize
                : Math.Min(request.MaxResults, maxPacketSize);
            var expand = request.Expand == null
                ? null
                : string.Join(",", request.Expand);
            return new JiraIssuesResponse
            {
                Total = issues.Length,
                StartAt = request.StartAt,
                MaxResults = maxResults,
                Expand = expand,
                Issues = issues.Skip(request.StartAt).Take(maxResults)
                    .Select(i =>
                    {
                        var queriedFields = Restrict(i.IssueFields, request.Fields);
                        var controller = new JiraIssueFieldsController(queriedFields);
                        return new JiraIssue(controller)
                        {
                            Key = i.Key,
                            Id = i.Id,
                            Self = i.Self,
                            Expand = expand,
                        };
                    })
                    .ToArray()
            };
        }

        public async Task<JiraIssueReference> CreateIssueAsync(JiraIssue fields,
            CancellationToken cancellationToken)
        {
            var project = fields.Project;
            if (project == (JiraProject) null)
                throw new JiraException("Field 'project' is required");
            var issueType = fields.IssueType;
            if (issueType == (JiraIssueType) null)
                throw new JiraException("Field 'issueType' is required");
            var key = $"{project.Key}-{await store.GenerateId()}";
            var id = await store.GenerateId();
            var jObject = fields.Controller.GetFields();
            if (jObject.GetProperty<DateTime>("created") == default)
                jObject.SetProperty("created", DateTime.Now);
            if (jObject.GetProperty<DateTime>("updated") == default)
                jObject.SetProperty("updated", DateTime.Now);
            if (jObject.GetProperty<JiraUser>("creator") == (JiraUser) null)
                jObject.SetProperty("creator", authorizedUser);
            var (workflow, _) = metadataProvider.GetWorkflow()
                .SingleOrDefault(x => x.workflow.IssueType == issueType);
            if (workflow != null)
            {
                if (jObject.GetProperty<JiraStatus>("status") == (JiraStatus) null &&
                    workflow.DefaultStatus != (JiraStatus) null)
                    jObject.SetProperty("status", workflow.DefaultStatus);
            }

            var issue = new JiraIssueDto
            {
                Key = key,
                Id = id.ToString(),
                Self = $"{fakeJiraUrl}/rest/api/2/issue/{id}",
                IssueFields = jObject
            };
            await store.Insert(issue);
            return new JiraIssueReference
            {
                Key = issue.Key,
                Id = issue.Id,
                Self = issue.Self,
            };
        }

        public async Task UpdateIssueAsync(JiraIssueReference reference,
            JiraIssue fields,
            CancellationToken cancellationToken)
        {
            var jObject = fields.Controller.GetChangedFields();
            jObject.SetProperty("updated", DateTime.Now);
            await store.Update(reference.Key ?? reference.Id, jObject);
        }

        public async Task<byte[]> DownloadAttachmentAsync(JiraAttachment attachment,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(attachment.Content))
                throw new JiraException("attachment's content is null or empty");
            var attachmentId = long.Parse(attachment.Id);
            return await store.DownloadAttachment(attachmentId);
        }

        public async Task<JiraAttachment> UploadAttachmentAsync(JiraIssueReference issueReference, string fileName,
            byte[] bytes,
            CancellationToken cancellationToken)
        {
            var identifier = await store.GenerateId();
            var attachment = new JiraAttachment
            {
                Id = identifier.ToString(),
                Created = DateTime.Now,
                Filename = fileName,
                Size = bytes.Length,
                Content = $"{fakeJiraUrl}/secure/attachment/{identifier}/{HttpUtility.UrlEncode(fileName)}",
                Self = $"{fakeJiraUrl}/rest/api/2/attachment/{identifier}"
            };
            await store.UploadAttachment(issueReference.Key ?? issueReference.Id, identifier, attachment, bytes);
            return attachment;
        }

        public async Task DeleteAttachmentAsync(JiraAttachment attachment, CancellationToken cancellationToken)
        {
            await store.DeleteAttachment(long.Parse(attachment.Id));
        }

        public async Task<JiraCommentsResponse> GetCommentsAsync(JiraIssueReference issueReference,
            JiraCommentsRequest request,
            CancellationToken cancellationToken)
        {
            var comments = await store.GetComments(issueReference.Key ?? issueReference.Id);
            var maxResults = request.MaxResults == 0
                ? maxPacketSize
                : Math.Min(request.MaxResults, maxPacketSize);
            return new JiraCommentsResponse
            {
                Comments = comments.Skip(request.StartAt).Take(maxResults).ToArray(),
                Total = comments.Length,
                MaxResults = maxResults,
                StartAt = request.StartAt
            };
        }

        public async Task<JiraComment> AddCommentAsync(JiraIssueReference issueReference, JiraComment comment,
            CancellationToken cancellationToken)
        {
            var identifier = await store.GenerateId();
            var result = new JiraComment
            {
                Author = comment.Author == (JiraUser) null ? authorizedUser : comment.Author,
                Body = comment.Body,
                Created = comment.Created == default ? DateTime.Now : comment.Created,
                Id = identifier.ToString(),
                Self = $"{fakeJiraUrl}/rest/api/2/issue/{issueReference.Id}/comment/{identifier}",
                Updated = comment.Updated == default ? DateTime.Now : comment.Updated,
                UpdateAuthor = comment.UpdateAuthor == (JiraUser) null ? authorizedUser : comment.UpdateAuthor
            };
            await store.AddComment(issueReference.Key ?? issueReference.Id, result);
            return result;
        }

        public async Task<JiraTransition[]> GetTransitionsAsync(JiraIssueReference issueReference,
            CancellationToken cancellationToken)
        {
            var issue = await store.Get(issueReference.Key ?? issueReference.Id);
            return GetTransitions(issue);
        }

        public async Task InvokeTransitionAsync(JiraIssueReference issueReference, string transitionId, object fields,
            CancellationToken cancellationToken)
        {
            var issue = await store.Get(issueReference.Key ?? issueReference.Id);
            var jiraTransitions = GetTransitions(issue);
            var currentTransition = jiraTransitions.SingleOrDefault(x =>
                string.Equals(x.Id, transitionId, StringComparison.InvariantCultureIgnoreCase));
            if (currentTransition == null)
                throw new JiraException(
                    $"transition '{transitionId}' for issue '{issueReference.Key}', id: '{issueReference.Id}' is not found");
            if (fields != null)
                throw new NotSupportedException();

            issue.IssueFields.SetProperty("status", currentTransition.To);
            await store.Update(issueReference.Key ?? issueReference.Id, issue.IssueFields);
        }

        public void Drop()
        {
            store.Drop();
        }

        private static JiraIssueFields Restrict(JiraIssueFields issueFields, IEnumerable<string> requestFields)
        {
            return requestFields == null
                ? issueFields
                : requestFields.Aggregate(new JiraIssueFields(), (res, cur) =>
                {
                    var property = issueFields.GetProperty(cur, typeof(object));
                    if (property != null)
                        res.SetProperty(cur, property);
                    return res;
                });
        }

        private JiraTransition[] GetTransitions(JiraIssueDto issue)
        {
            var workflowList = metadataProvider.GetWorkflow();
            var currentWorkflow =
                workflowList.SingleOrDefault(x =>
                    x.workflow.IssueType == issue.IssueFields.GetProperty<JiraIssueType>("issueType"));
            var jiraIssue = new JiraIssue(new JiraIssueFieldsController(issue.IssueFields))
            {
                Key = issue.Key,
                Id = issue.Id,
                Self = issue.Self
            }.Cast(currentWorkflow.type);

            if (currentWorkflow == default || jiraIssue.Status == (JiraStatus) null)
                return new JiraTransition[0];

            return currentWorkflow.workflow.GetTransitions(jiraIssue.Status)
                .Select(x => (transitions: x, condition: currentWorkflow.workflow.GetCondition(x)))
                .Where(x => x.condition(jiraIssue))
                .Select(x => x.transitions)
                .ToArray();
        }
    }
}