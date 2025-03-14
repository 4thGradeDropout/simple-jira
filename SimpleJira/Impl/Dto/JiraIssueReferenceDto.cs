using System.Text.Json.Serialization;

namespace SimpleJira.Impl.Dto
{
    internal class JiraIssueReferenceDto
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("key")] public string Key { get; set; }
        [JsonPropertyName("self")] public string Self { get; set; }
    }
}