using Newtonsoft.Json;

namespace SimpleJira.Impl.Dto
{
    internal class JiraUserDto
    {
        [JsonProperty("active")] public bool Active { get; set; }
        [JsonProperty("avatarUrls")] public JiraAvatarUrlsDto AvatarUrls { get; set; }
        [JsonProperty("displayName")] public string DisplayName { get; set; }
        [JsonProperty("emailAddress")] public string EmailAddress { get; set; }
        [JsonProperty("key")] public string Key { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("self")] public string Self { get; set; }
        [JsonProperty("timeZone")] public string TimeZone { get; set; }
    }
}