using System.Text.Json.Serialization;

namespace SimpleJira.Impl.Dto
{
    internal class JiraProjectDto
    {
        [JsonPropertyName("avatarUrls")] public JiraAvatarUrlsDto AvatarUrls { get; set; }
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("key")] public string Key { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("projectTypeKey")] public string ProjectTypeKey { get; set; }
        [JsonPropertyName("self")] public string Self { get; set; }
    }
}