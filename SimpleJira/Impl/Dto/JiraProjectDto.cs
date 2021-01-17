using Newtonsoft.Json;

namespace SimpleJira.Impl.Dto
{
    internal class JiraProjectDto
    {
        [JsonProperty("avatarUrls")] public JiraAvatarUrlsDto AvatarUrls { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("key")] public string Key { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("projectTypeKey")] public string ProjectTypeKey { get; set; }
        [JsonProperty("self")] public string Self { get; set; }
    }
}