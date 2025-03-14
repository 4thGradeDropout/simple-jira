using System.Text.Json.Serialization;

namespace SimpleJira.Impl.Dto
{
    internal class JiraCustomFieldOptionDto
    {
        [JsonPropertyName("child")] public JiraCustomFieldOptionDto Child { get; set; }
        [JsonPropertyName("self")] public string Self { get; set; }
        [JsonPropertyName("value")] public string Value { get; set; }
        [JsonPropertyName("id")] public string Id { get; set; }
    }
}