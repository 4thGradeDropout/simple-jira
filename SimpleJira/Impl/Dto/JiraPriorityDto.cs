using System.Text.Json.Serialization;

namespace SimpleJira.Impl.Dto
{
    internal class JiraPriorityDto
    {
        [JsonPropertyName("iconUrl")] public string IconUrl { get; set; }
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("self")] public string Self { get; set; }
    }
}