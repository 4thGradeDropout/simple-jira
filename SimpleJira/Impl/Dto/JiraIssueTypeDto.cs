using System.Text.Json.Serialization;

namespace SimpleJira.Impl.Dto
{
    internal class JiraIssueTypeDto
    {
        [JsonPropertyName("description")] public string Description { get; set; }
        [JsonPropertyName("iconUrl")] public string IconUrl { get; set; }
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("self")] public string Self { get; set; }
        [JsonPropertyName("subtask")] public bool SubTask { get; set; }
    }
}