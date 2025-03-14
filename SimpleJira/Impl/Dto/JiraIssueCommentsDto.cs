using System.Text.Json.Serialization;

namespace SimpleJira.Impl.Dto
{
    internal class JiraIssueCommentsDto
    {
        [JsonPropertyName("comments")] public JiraCommentDto[] Comments { get; set; }
        [JsonPropertyName("maxResults")] public int MaxResults { get; set; }
        [JsonPropertyName("startAt")] public int StartAt { get; set; }
        [JsonPropertyName("total")] public int Total { get; set; }
    }
}