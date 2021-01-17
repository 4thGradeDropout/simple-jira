using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Interface
{
    public static class JiraExtensions
    {
        /// <summary>
        /// Does issues' search by JQL.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="request">Searching request.</param>
        /// <param name="issueType">Type of an issue to return.</param>
        /// <exception cref="SimpleJira.Interface.JqlCompilationException">Throws exception when JQL is not valid.</exception>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Search response which contains issues, issues' count, additional information from request.
        /// </returns>
        public static JiraIssuesResponse SelectIssues(this IJira jira, JiraIssuesRequest request, Type issueType)
        {
            return jira.SelectIssuesAsync(request, issueType, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Does issues' search by JQL.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="request">Searching request.</param>
        /// <exception cref="SimpleJira.Interface.JqlCompilationException">Throws exception when JQL is not valid.</exception>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Search response which contains issues, issues' count, additional information from request.
        /// </returns>
        public static Task<JiraIssuesResponse> SelectIssuesAsync(this IJira jira, JiraIssuesRequest request)
        {
            return jira.SelectIssuesAsync(request, CancellationToken.None);
        }

        /// <summary>
        /// Does issues' search by JQL.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="request">Searching request.</param>
        /// <exception cref="SimpleJira.Interface.JqlCompilationException">Throws exception when JQL is not valid.</exception>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Typified search response which contains typified issues, issues' count, additional information from request.
        /// </returns>
        public static JiraIssuesResponse<TIssue> SelectIssues<TIssue>(this IJira jira,
            JiraIssuesRequest request) where TIssue : JiraIssue
        {
            return jira.SelectIssuesAsync<TIssue>(request, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Does issues' search by JQL.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="request">Searching request.</param>
        /// <exception cref="SimpleJira.Interface.JqlCompilationException">Throws exception when JQL is not valid.</exception>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Typified search response which contains typified issues, issues' count, additional information from request.
        /// </returns>
        public static Task<JiraIssuesResponse<TIssue>> SelectIssuesAsync<TIssue>(this IJira jira,
            JiraIssuesRequest request) where TIssue : JiraIssue
        {
            return jira.SelectIssuesAsync<TIssue>(request, CancellationToken.None);
        }

        /// <summary>
        /// Does issues' search by JQL.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="request">Searching request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="SimpleJira.Interface.JqlCompilationException">Throws exception when JQL is not valid.</exception>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Typified search response which contains typified issues, issues' count, additional information from request.
        /// </returns>
        public static async Task<JiraIssuesResponse<TIssue>> SelectIssuesAsync<TIssue>(this IJira jira,
            JiraIssuesRequest request, CancellationToken cancellationToken) where TIssue : JiraIssue
        {
            var result = await jira.SelectIssuesAsync(request, typeof(TIssue), cancellationToken);
            return new JiraIssuesResponse<TIssue>
            {
                Expand = result.Expand,
                Issues = result.Issues.Cast<TIssue>().ToArray(),
                Total = result.Total,
                MaxResults = result.MaxResults,
                StartAt = result.StartAt
            };
        }

        /// <summary>
        /// Does issues' search by JQL.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="request">Searching request.</param>
        /// <param name="issueType">Type of an issue to return.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="SimpleJira.Interface.JqlCompilationException">Throws exception when JQL is not valid.</exception>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Search response which contains issues, issues' count, additional information from request.
        /// </returns>
        public static async Task<JiraIssuesResponse> SelectIssuesAsync(this IJira jira,
            JiraIssuesRequest request, Type issueType, CancellationToken cancellationToken)
        {
            var response = await jira.SelectIssuesAsync(new JiraIssuesRequest
            {
                Expand = request.Expand,
                Fields = request.Fields,
                Jql = request.Jql,
                MaxResults = request.MaxResults,
                StartAt = request.StartAt,
                ValidateQuery = request.ValidateQuery
            }, cancellationToken);
            if (issueType == typeof(JiraIssue))
                return response;
            return new JiraIssuesResponse
            {
                Expand = response.Expand,
                Total = response.Total,
                MaxResults = response.MaxResults,
                StartAt = response.StartAt,
                Issues = response.Issues.Select(x => x.Cast(issueType)).ToArray()
            };
        }

        /// <summary>
        /// Creates new issue and returns reference to created issue.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="issue">Issue.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Reference To issue that contains issue's key, issue's id, issue's API url.
        /// </returns>
        public static JiraIssueReference CreateIssue(this IJira jira, JiraIssue issue)
        {
            return jira.CreateIssueAsync(issue, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Creates new issue and returns reference to created issue.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="issue">Issue.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Reference To issue that contains issue's key, issue's id, issue's API url.
        /// </returns>
        public static Task<JiraIssueReference> CreateIssueAsync(this IJira jira, JiraIssue issue)
        {
            return jira.CreateIssueAsync(issue, CancellationToken.None);
        }

        /// <summary>
        /// Does update an existing issue.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="issue">An issue that contains fields to update.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        public static void UpdateIssue(this IJira jira, JiraIssueReference issueReference, JiraIssue issue)
        {
            jira.UpdateIssueAsync(issueReference, issue, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Does update an existing issue.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="issue">An issue that contains fields to update.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        public static Task UpdateIssueAsync(this IJira jira, JiraIssueReference issueReference, JiraIssue issue)
        {
            return jira.UpdateIssueAsync(issueReference, issue, CancellationToken.None);
        }

        /// <summary>
        /// Uploads attachment to an existing issue.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="fileName">Filename of the attachment.</param>
        /// <param name="bytes">Content of the attachment.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Attachment's model that contains id of the attachment, URL to download, filename of attachment, others.
        /// </returns>
        public static JiraAttachment UploadAttachment(this IJira jira, JiraIssueReference issueReference,
            string fileName, byte[] bytes)
        {
            return jira.UploadAttachmentAsync(issueReference, fileName, bytes, CancellationToken.None).GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Uploads attachment to an existing issue.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="fileName">Filename of the attachment.</param>
        /// <param name="bytes">Content of the attachment.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Attachment's model that contains id of the attachment, URL to download, filename of attachment, others.
        /// </returns>
        public static Task<JiraAttachment> UploadAttachmentAsync(this IJira jira, JiraIssueReference issueReference,
            string fileName, byte[] bytes)
        {
            return jira.UploadAttachmentAsync(issueReference, fileName, bytes, CancellationToken.None);
        }

        /// <summary>
        /// Downloads an issue's attachment.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="attachment">Attachment's model. Should contains content url to download.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Attachment's content.
        /// </returns>
        public static byte[] DownloadAttachment(this IJira jira, JiraAttachment attachment)
        {
            return jira.DownloadAttachmentAsync(attachment, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Downloads an issue's attachment.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="attachment">Attachment's model. Should contains content url to download.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Attachment's content.
        /// </returns>
        public static Task<byte[]> DownloadAttachmentAsync(this IJira jira, JiraAttachment attachment)
        {
            return jira.DownloadAttachmentAsync(attachment, CancellationToken.None);
        }

        /// <summary>
        /// Deletes an issue's attachment.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="attachment">Attachment's model. Should contains content url to download.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        public static void DeleteAttachment(this IJira jira, JiraAttachment attachment)
        {
            jira.DeleteAttachmentAsync(attachment, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Deletes an issue's attachment.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="attachment">Attachment's model. Should contains content url to download.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        public static Task DeleteAttachmentAsync(this IJira jira, JiraAttachment attachment)
        {
            return jira.DeleteAttachmentAsync(attachment, CancellationToken.None);
        }

        /// <summary>
        /// Gets an issue's comments.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="request">Request that contains information about a required window.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Response that contains comments, comments' count, additional information from request.
        /// </returns>
        public static JiraCommentsResponse GetComments(this IJira jira, JiraIssueReference issueReference,
            JiraCommentsRequest request)
        {
            return jira.GetCommentsAsync(issueReference, request, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets an issue's comments.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="request">Request that contains information about a required window.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Response that contains comments, comments' count, additional information from request.
        /// </returns>
        public static Task<JiraCommentsResponse> GetCommentsAsync(this IJira jira, JiraIssueReference issueReference,
            JiraCommentsRequest request)
        {
            return jira.GetCommentsAsync(issueReference, request, CancellationToken.None);
        }

        /// <summary>
        /// Adds comment to an existing issue.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="comment">Comment model. Should contains Body.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Comment model with comment's id, comment's API url, comment's author, others.
        /// </returns>
        public static JiraComment AddComment(this IJira jira, JiraIssueReference issueReference,
            JiraComment comment)
        {
            return jira.AddCommentAsync(issueReference, comment, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds comment to an existing issue.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="comment">Comment model. Should contains Body.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Comment model with comment's id, comment's API url, comment's author, others.
        /// </returns>
        public static Task<JiraComment> AddCommentAsync(this IJira jira, JiraIssueReference issueReference,
            JiraComment comment)
        {
            return jira.AddCommentAsync(issueReference, comment, CancellationToken.None);
        }

        /// <summary>
        /// Gets transitions' list of an existing issue.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	List of issue's transitions.
        /// </returns>
        public static JiraTransition[] GetTransitions(this IJira jira, JiraIssueReference issueReference)
        {
            return jira.GetTransitionsAsync(issueReference, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets transitions' list of an existing issue.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	List of issue's transitions.
        /// </returns>
        public static Task<JiraTransition[]> GetTransitionsAsync(this IJira jira, JiraIssueReference issueReference)
        {
            return jira.GetTransitionsAsync(issueReference, CancellationToken.None);
        }

        /// <summary>
        /// Invokes issue's transition.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="transitionId">Identifier of the transition.</param>
        /// <param name="fields">Fields to update.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        public static void InvokeTransition(this IJira jira, JiraIssueReference issueReference,
            string transitionId, object fields)
        {
            jira.InvokeTransitionAsync(issueReference, transitionId, fields, CancellationToken.None).GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Invokes issue's transition.
        /// </summary>
        /// <param name="jira">Instance of <c>SimpleJira.Interface.IJira</c>.</param>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="transitionId">Identifier of the transition.</param>
        /// <param name="fields">Fields to update.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        public static Task InvokeTransitionAsync(this IJira jira, JiraIssueReference issueReference,
            string transitionId, object fields)
        {
            return jira.InvokeTransitionAsync(issueReference, transitionId, fields, CancellationToken.None);
        }
    }
}