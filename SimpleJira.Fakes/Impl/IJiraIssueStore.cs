using System.Threading.Tasks;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Fakes.Impl
{
    internal interface IJiraIssueStore
    {
        Task<long> GenerateId();
        Task<JiraIssueDto> Get(string keyOrId);
        Task<JiraIssueDto[]> GetAll();
        Task Insert(JiraIssueDto jObject);
        Task Update(string keyOrId, JiraIssueFields fields);
        Task UploadAttachment(string keyOrId, long attachmentId, JiraAttachment attachment, byte[] content);
        Task<byte[]> DownloadAttachment(long attachmentId);
        Task DeleteAttachment(long attachmentId);
        Task<JiraComment[]> GetComments(string keyOrId);
        Task AddComment(string keyOrId, JiraComment comment);
        void Drop();
    }
}