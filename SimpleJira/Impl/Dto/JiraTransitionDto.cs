using System.Text.Json.Serialization;

namespace SimpleJira.Impl.Dto
{
    internal class JiraTransitionDto
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("to")] public JiraStatusDto To { get; set; }
    }
}