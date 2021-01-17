using Newtonsoft.Json;

namespace SimpleJira.Impl.Dto
{
    internal class JiraIssueCommentsDto
    {
        [JsonProperty("comments")] public JiraCommentDto[] Comments { get; set; }
        [JsonProperty("maxResults")] public int MaxResults { get; set; }
        [JsonProperty("startAt")] public int StartAt { get; set; }
        [JsonProperty("total")] public int Total { get; set; }
    }
}