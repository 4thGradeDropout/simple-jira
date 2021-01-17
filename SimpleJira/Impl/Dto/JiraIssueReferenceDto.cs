using Newtonsoft.Json;

namespace SimpleJira.Impl.Dto
{
    internal class JiraIssueReferenceDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("key")] public string Key { get; set; }
        [JsonProperty("self")] public string Self { get; set; }
    }
}