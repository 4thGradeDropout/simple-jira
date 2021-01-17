using System.Threading;
using System.Threading.Tasks;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Interface
{
    public interface IJira
    {
        /// <summary>
        /// Does issues' search by JQL.
        /// </summary>
        /// <param name="request">Searching request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="SimpleJira.Interface.JqlCompilationException">Throws exception when JQL is not valid.</exception>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	search response that contains issues, issues' count, additional information from request.
        /// </returns>
        Task<JiraIssuesResponse> SelectIssuesAsync(JiraIssuesRequest request, CancellationToken cancellationToken);

        /// <summary>
        /// Creates new issue and returns reference to created issue.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Reference To issue that contains issue's key, issue's id, issue's API url.
        /// </returns>
        Task<JiraIssueReference> CreateIssueAsync(JiraIssue issue, CancellationToken cancellationToken);

        /// <summary>
        /// Does update an existing issue.
        /// </summary>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="issue">An issue that contains fields to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        Task UpdateIssueAsync(JiraIssueReference issueReference, JiraIssue issue, CancellationToken cancellationToken);

        /// <summary>
        /// Uploads attachment to an existing issue.
        /// </summary>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="fileName">Filename of the attachment.</param>
        /// <param name="bytes">Content of the attachment.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Attachment's model that contains id of the attachment, URL to download, filename of attachment, others.
        /// </returns>
        Task<JiraAttachment> UploadAttachmentAsync(JiraIssueReference issueReference, string fileName, byte[] bytes,
            CancellationToken cancellationToken);

        /// <summary>
        /// Downloads an issue's attachment.
        /// </summary>
        /// <param name="attachment">Attachment's model. Should contains content url to download.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Attachment's content.
        /// </returns>
        Task<byte[]> DownloadAttachmentAsync(JiraAttachment attachment, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes an issue's attachment.
        /// </summary>
        /// <param name="attachment">Attachment's model. Should contains content url to download.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        Task DeleteAttachmentAsync(JiraAttachment attachment, CancellationToken cancellationToken);

        /// <summary>
        /// Gets an issue's comments.
        /// </summary>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="request">Request that contains information about a required window.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Response that contains comments, comments' count, additional information from request.
        /// </returns>
        Task<JiraCommentsResponse> GetCommentsAsync(JiraIssueReference issueReference, JiraCommentsRequest request,
            CancellationToken cancellationToken);

        /// <summary>
        /// Adds comment to an existing issue.
        /// </summary>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="comment">Comment model. Should contains Body.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	Comment model with comment's id, comment's API url, comment's author, others.
        /// </returns>
        Task<JiraComment> AddCommentAsync(JiraIssueReference issueReference, JiraComment comment,
            CancellationToken cancellationToken);

        /// <summary>
        /// Gets transitions' list of an existing issue.
        /// </summary>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        /// <returns>
        /// 	List of issue's transitions.
        /// </returns>
        Task<JiraTransition[]> GetTransitionsAsync(JiraIssueReference issueReference,
            CancellationToken cancellationToken);

        /// <summary>
        /// Invokes issue's transition.
        /// </summary>
        /// <param name="issueReference">Reference to an existing issue. Should contains issue's key or issue's id.</param>
        /// <param name="transitionId">Identifier of the transition.</param>
        /// <param name="fields">Fields to update.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="SimpleJira.Interface.JiraAuthorizationException">Throws exception when user is not authorized.</exception>
        /// <exception cref="SimpleJira.Interface.JiraException">Throws exception in other cases.</exception>
        Task InvokeTransitionAsync(JiraIssueReference issueReference, string transitionId, object fields,
            CancellationToken cancellationToken);
    }
}