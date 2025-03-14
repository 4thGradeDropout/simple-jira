using System.Text.Json.Serialization;

namespace SimpleJira.Impl.Dto
{
    internal class JiraUserDto
    {
        [JsonPropertyName("active")] public bool Active { get; set; }
        [JsonPropertyName("avatarUrls")] public JiraAvatarUrlsDto AvatarUrls { get; set; }
        [JsonPropertyName("displayName")] public string DisplayName { get; set; }
        [JsonPropertyName("emailAddress")] public string EmailAddress { get; set; }
        [JsonPropertyName("key")] public string Key { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("self")] public string Self { get; set; }
        [JsonPropertyName("timeZone")] public string TimeZone { get; set; }
    }
}