using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Text.Json.Serialization;
using SimpleJira.Impl.Controllers;
using SimpleJira.Impl.Dto;
using SimpleJira.Impl.Serialization;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Impl.RestApi
{
    internal class RestApiJira : IJira
    {
        private readonly string hostUrl;
        private readonly HttpClient jira;

        public RestApiJira(string hostUrl, string user, string password)
        {
            this.hostUrl = hostUrl[hostUrl.Length - 1] == '/'
                ? hostUrl.Substring(0, hostUrl.Length - 1)
                : hostUrl;
            jira = new HttpClient();
            jira.DefaultRequestHeaders.Authorization = BasicAuthenticationHeader.Value(user, password);
            jira.DefaultRequestHeaders.Add("X-Atlassian-Token", "nocheck");
        }

        public async Task<JiraIssuesResponse> SelectIssuesAsync(JiraIssuesRequest request,
            CancellationToken cancellationToken)
        {
            var requestModel = AutoMapper.Create<JiraIssuesRequest, JiraSearchRequestModel>().Map(request);
            var requestJson = Json.Serialize(requestModel);
            cancellationToken.ThrowIfCancellationRequested();
            var responseMessage = await jira.PostAsync(hostUrl + "/rest/api/2/search",
                new StringContent(requestJson, Encoding.UTF8, "application/json"),
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!responseMessage.IsSuccessStatusCode)
            {
                switch (responseMessage.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                    {
                        var body = await responseMessage.Content.ReadAsStringAsync();
                        cancellationToken.ThrowIfCancellationRequested();
                        ThrowBadRequest(body, $"select issues failed, jql '{request.Jql}'");
                        break;
                    }
                    case HttpStatusCode.Unauthorized:
                        throw new JiraAuthorizationException();
                    default:
                        responseMessage.EnsureSuccessStatusCode();
                        break;
                }
            }

            var responseJson = await responseMessage.Content.ReadAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();
            var response = Json.Deserialize<JiraSearchResponseModel>(responseJson);
            return new JiraIssuesResponse
            {
                Expand = response.Expand,
                Total = response.Total,
                MaxResults = response.MaxResults,
                StartAt = response.StartAt,
                Issues = response.Issues.Select(ToIssue).ToArray()
            };
        }

        public async Task<JiraIssueReference> CreateIssueAsync(JiraIssue fields,
            CancellationToken cancellationToken)
        {
            var project = fields.Controller.GetValue<JiraProject>("project");
            if (project == (JiraProject)null)
                throw new JiraException("Field 'project' is required");
            var request = fields.Controller.GetFields();
            var requestJson = $"{{\"fields\": {request.ToJson()}}}";
            cancellationToken.ThrowIfCancellationRequested();
            var responseMessage = await jira.PostAsync(hostUrl + "/rest/api/2/issue",
                new StringContent(requestJson, Encoding.UTF8, "application/json"),
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!responseMessage.IsSuccessStatusCode)
                switch (responseMessage.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        var body = await responseMessage.Content.ReadAsStringAsync();
                        cancellationToken.ThrowIfCancellationRequested();
                        ThrowBadRequest(body, "create issue failed. " + request.ToJson());
                        break;
                    case HttpStatusCode.Unauthorized:
                        throw new JiraAuthorizationException();
                    default:
                        responseMessage.EnsureSuccessStatusCode();
                        break;
                }

            var responseJson = await responseMessage.Content.ReadAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();
            return Json.Deserialize<JiraIssueReference>(responseJson);
        }

        public async Task UpdateIssueAsync(JiraIssueReference key, JiraIssue issue,
            CancellationToken cancellationToken)
        {
            var identifier = key.Key ?? key.Id;
            if (identifier == null)
                throw new JiraException("issue's key and issue's id are null");
            var request = issue.Controller.GetChangedFields();
            var requestJson = $"{{\"fields\": {request.ToJson()}}}";
            cancellationToken.ThrowIfCancellationRequested();
            var responseMessage = await jira.PutAsync(hostUrl + "/rest/api/2/issue/" + identifier,
                new StringContent(requestJson, Encoding.UTF8, "application/json"),
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!responseMessage.IsSuccessStatusCode)
            {
                switch (responseMessage.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        var body = await responseMessage.Content.ReadAsStringAsync();
                        cancellationToken.ThrowIfCancellationRequested();
                        ThrowBadRequest(body, $"update issue '{identifier}' failed: " + request.ToJson());
                        break;
                    case HttpStatusCode.Unauthorized:
                        throw new JiraAuthorizationException();
                    case HttpStatusCode.Forbidden:
                        throw new JiraException(
                            "user uses 'overrideScreenSecurity' or 'overrideEditableFlag' but doesn't have the necessary permission");
                    case HttpStatusCode.NotFound:
                        throw new JiraException(
                            $"issue '{identifier}' is not found or the user does not have permission to view it");
                    default:
                        responseMessage.EnsureSuccessStatusCode();
                        break;
                }
            }
        }

        public async Task<byte[]> DownloadAttachmentAsync(JiraAttachment attachment,
            CancellationToken cancellationToken)
        {
            string url;
            if (!string.IsNullOrEmpty(attachment.Content))
                url = attachment.Content;
            else if (!string.IsNullOrEmpty(attachment.Id))
            {
                var fileName = attachment.Filename ?? "attachment.bin";
                url = $"{hostUrl}/secure/attachment/{attachment.Id}/{HttpUtility.UrlEncode(fileName)}";
            }
            else
                throw new JiraException("attachment's content is null or empty");

            var responseMessage = await jira.GetAsync(url, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!responseMessage.IsSuccessStatusCode)
            {
                switch (responseMessage.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        var body = await responseMessage.Content.ReadAsStringAsync();
                        cancellationToken.ThrowIfCancellationRequested();
                        ThrowBadRequest(body, $"download attachment '{url}' failed.");
                        break;
                    case HttpStatusCode.Unauthorized:
                        throw new JiraAuthorizationException();
                    case HttpStatusCode.NotFound:
                        throw new JiraException($"attachment '{attachment.Id}' is not found");
                    default:
                        responseMessage.EnsureSuccessStatusCode();
                        break;
                }
            }

            return await responseMessage.Content.ReadAsByteArrayAsync();
        }

        public async Task<JiraAttachment> UploadAttachmentAsync(JiraIssueReference issueReference,
            string fileName,
            byte[] bytes, CancellationToken cancellationToken)
        {
            var identifier = issueReference.Key ?? issueReference.Id;
            if (identifier == null)
                throw new JiraException("issue's key and issue's id are null");
            var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(bytes);
            content.Add(fileContent, "file", fileName);
            var responseMessage = await jira.PostAsync($"{hostUrl}/rest/api/2/issue/{identifier}/attachments",
                content, cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                switch (responseMessage.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        var body = await responseMessage.Content.ReadAsStringAsync();
                        cancellationToken.ThrowIfCancellationRequested();
                        ThrowBadRequest(body, $"upload attachment '{fileName}' to '{identifier}' failed.");
                        break;
                    case HttpStatusCode.Unauthorized:
                        throw new JiraAuthorizationException();
                    case HttpStatusCode.Forbidden:
                        throw new JiraException("user does not have the necessary permission");
                    case HttpStatusCode.NotFound:
                        throw new JiraException(
                            $"issue '{identifier}' is not found or the user does not have permission to view the issue");
                    case HttpStatusCode.RequestEntityTooLarge:
                        throw new JiraException(
                            $"attachments exceed the maximum attachment size for issues, filename [{fileName}]");
                    default:
                        responseMessage.EnsureSuccessStatusCode();
                        break;
                }
            }

            var responseBody = await responseMessage.Content.ReadAsStringAsync();
            return Json.Deserialize<JiraAttachment[]>(responseBody)[0];
        }

        public async Task DeleteAttachmentAsync(JiraAttachment attachment, CancellationToken cancellationToken)
        {
            var id = attachment.Id;
            var responseMessage = await jira.DeleteAsync($"{hostUrl}/rest/api/2/attachment/{id}", cancellationToken);
            if (!responseMessage.IsSuccessStatusCode)
            {
                switch (responseMessage.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        var body = await responseMessage.Content.ReadAsStringAsync();
                        cancellationToken.ThrowIfCancellationRequested();
                        ThrowBadRequest(body, $"delete attachment '{id}' failed.");
                        break;
                    case HttpStatusCode.Unauthorized:
                        throw new JiraAuthorizationException();
                    case HttpStatusCode.Forbidden:
                        throw new JiraException("user does not have the necessary permission");
                    case HttpStatusCode.NotFound:
                        throw new JiraException(
                            $"attachment '{id}' is not found or the user does not have permission to view the issue");
                    default:
                        responseMessage.EnsureSuccessStatusCode();
                        break;
                }
            }
        }

        public async Task<JiraCommentsResponse> GetCommentsAsync(JiraIssueReference issueReference,
            JiraCommentsRequest request, CancellationToken cancellationToken)
        {
            var identifier = issueReference.Key ?? issueReference.Id;
            if (identifier == null)
                throw new JiraException("issue's key and issue's id are null");
            var maxResults = request.MaxResults == 0 ? 50 : request.MaxResults;
            var responseMessage = await jira.GetAsync(
                $"{hostUrl}/rest/api/2/issue/{identifier}/comment?startAt={request.StartAt}&maxResults={maxResults}",
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!responseMessage.IsSuccessStatusCode)
            {
                switch (responseMessage.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        var body = await responseMessage.Content.ReadAsStringAsync();
                        cancellationToken.ThrowIfCancellationRequested();
                        ThrowBadRequest(body, $"get comments of '{identifier}' failed.");
                        break;
                    case HttpStatusCode.Unauthorized:
                        throw new JiraAuthorizationException();
                    case HttpStatusCode.NotFound:
                        throw new JiraException(
                            $"issue '{identifier}' is not found or the user does not have permission to view it");
                    default:
                        responseMessage.EnsureSuccessStatusCode();
                        break;
                }
            }

            var responseBody = await responseMessage.Content.ReadAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();
            var dto = Json.Deserialize<JiraCommentsResponseDto>(responseBody);
            return (JiraCommentsResponse)AutoMapper.Create<JiraCommentsResponseDto, JiraCommentsResponse>().Map(dto);
        }

        public async Task<JiraComment> AddCommentAsync(JiraIssueReference issueReference, JiraComment comment,
            CancellationToken cancellationToken)
        {
            var identifier = issueReference.Key ?? issueReference.Id;
            if (identifier == null)
                throw new JiraException("issue's key and issue's id are null");
            var requestJson = Json.Serialize(comment);
            cancellationToken.ThrowIfCancellationRequested();
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            var responseMessage = await jira.PostAsync($"{hostUrl}/rest/api/2/issue/{identifier}/comment", content,
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (!responseMessage.IsSuccessStatusCode)
            {
                switch (responseMessage.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        var body = await responseMessage.Content.ReadAsStringAsync();
                        cancellationToken.ThrowIfCancellationRequested();
                        ThrowBadRequest(body, $"add comment to '{identifier}' failed, text '{comment}'.");
                        break;
                    case HttpStatusCode.Unauthorized:
                        throw new JiraAuthorizationException();
                    case HttpStatusCode.NotFound:
                        throw new JiraException(
                            $"issue '{identifier}' is not found or the user does not have permission to view it");
                    default:
                        responseMessage.EnsureSuccessStatusCode();
                        break;
                }
            }

            var responseBody = await responseMessage.Content.ReadAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();
            return Json.Deserialize<JiraComment>(responseBody);
        }

        public async Task<JiraTransition[]> GetTransitionsAsync(JiraIssueReference issueReference,
            CancellationToken cancellationToken)
        {
            var identifier = issueReference.Key ?? issueReference.Id;
            if (identifier == null)
                throw new JiraException("issue's key and issue's id are null");

            var responseMessage = await jira.GetAsync($"{hostUrl}/rest/api/2/issue/{identifier}/transitions",
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            if (!responseMessage.IsSuccessStatusCode)
            {
                switch (responseMessage.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        var body = await responseMessage.Content.ReadAsStringAsync();
                        cancellationToken.ThrowIfCancellationRequested();
                        ThrowBadRequest(body, $"get transitions of '{identifier}' failed.");
                        break;
                    case HttpStatusCode.Unauthorized:
                        throw new JiraAuthorizationException();
                    case HttpStatusCode.NotFound:
                        throw new JiraException(
                            $"issue '{identifier}' is not found or the user does not have permission to view it");
                    default:
                        responseMessage.EnsureSuccessStatusCode();
                        break;
                }
            }

            var responseBody = await responseMessage.Content.ReadAsStringAsync();
            cancellationToken.ThrowIfCancellationRequested();
            var jiraTransitionDtos = Json.Deserialize<JiraTransitionsResponse>(responseBody).Transitions;
            var autoMapper = AutoMapper.Create<JiraTransitionDto, JiraTransition>();
            return jiraTransitionDtos.Select(x => (JiraTransition)autoMapper.Map(x)).ToArray();
        }

        public async Task InvokeTransitionAsync(JiraIssueReference issueReference, string transitionId, object fields,
            CancellationToken cancellationToken)
        {
            var identifier = issueReference.Key ?? issueReference.Id;
            if (identifier == null)
                throw new JiraException("issue's key and issue's id are null");
            var invocation = new JiraTransitionInvocation
            {
                Fields = fields,
                Transition = new JiraTransitionDto { Id = transitionId }
            };
            var requestJson = Json.Serialize(invocation);
            var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            cancellationToken.ThrowIfCancellationRequested();

            var responseMessage = await jira.PostAsync($"{hostUrl}/rest/api/2/issue/{identifier}/transitions", content,
                cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            if (!responseMessage.IsSuccessStatusCode)
            {
                switch (responseMessage.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        throw new JiraAuthorizationException();
                    case HttpStatusCode.BadRequest:
                        var body = await responseMessage.Content.ReadAsStringAsync();
                        cancellationToken.ThrowIfCancellationRequested();
                        ThrowBadRequest(body, $"can not invoke transition '{transitionId}' for issue '{identifier}'.");
                        break;
                    case HttpStatusCode.NotFound:
                        throw new JiraException(
                            $"issue '{identifier}' is not found or the user does not have permission to view it");
                    default:
                        responseMessage.EnsureSuccessStatusCode();
                        break;
                }
            }
        }

        private static void ThrowBadRequest(string body, string message)
        {
            JiraErrorMessages errors = null;
            try
            {
                errors = Json.Deserialize<JiraErrorMessages>(body);
            }
            catch
            {
                // ignored
            }

            var errorMessages = errors?.ErrorMessages ?? new string[0];
            if (errorMessages.Length == 0)
                errorMessages = new[] { body };
            throw new JqlCompilationException(message + "\n" + string.Join("\n", errorMessages));
        }

        private static JiraIssue ToIssue(JiraApiIssueModel issue)
        {
            return new JiraIssue(new JiraIssueFieldsController(new JiraIssueFields(issue.Fields)))
            {
                Expand = issue.Expand,
                Id = issue.Id,
                Key = issue.Key,
                Self = issue.Self
            };
        }

        private class JiraSearchRequestModel
        {
            [JsonPropertyName("jql")] public string Jql { get; set; }
            [JsonPropertyName("startAt")] public int StartAt { get; set; }
            [JsonPropertyName("maxResults")] public int MaxResults { get; set; }
            [JsonPropertyName("fields")] public string[] Fields { get; set; }
            [JsonPropertyName("validateQuery")] public bool ValidateQuery { get; set; }
            [JsonPropertyName("expand")] public string[] Expand { get; set; }
        }

        private class JiraSearchResponseModel
        {
            [JsonPropertyName("expand")] public string Expand { get; set; }
            [JsonPropertyName("issues")] public JiraApiIssueModel[] Issues { get; set; }
            [JsonPropertyName("maxResults")] public int MaxResults { get; set; }
            [JsonPropertyName("startAt")] public int StartAt { get; set; }
            [JsonPropertyName("total")] public int Total { get; set; }
        }

        private class JiraTransitionsResponse
        {
            [JsonPropertyName("expand")] public string Expand { get; set; }
            [JsonPropertyName("transitions")] public JiraTransitionDto[] Transitions { get; set; }
        }

        private class JiraTransitionInvocation
        {
            [JsonPropertyName("fields")] public object Fields { get; set; }
            [JsonPropertyName("transition")] public JiraTransitionDto Transition { get; set; }
        }

        private class JiraErrorMessages
        {
            [JsonPropertyName("errorMessages")] public string[] ErrorMessages { get; set; }
            [JsonPropertyName("errors")] public Errors Errors { get; set; }
        }

        private class Errors
        {
            [JsonPropertyName("status")] public string Status { get; set; }
        }

        private class JiraCommentsResponseDto
        {
            [JsonPropertyName("comments")] public JiraCommentDto[] Comments { get; set; }
            [JsonPropertyName("maxResults")] public int MaxResults { get; set; }
            [JsonPropertyName("startAt")] public int StartAt { get; set; }
            [JsonPropertyName("total")] public int Total { get; set; }
        }
    }
}