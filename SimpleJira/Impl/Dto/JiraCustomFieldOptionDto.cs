using Newtonsoft.Json;

namespace SimpleJira.Impl.Dto
{
    internal class JiraCustomFieldOptionDto
    {
        [JsonProperty("child")] public JiraCustomFieldOptionDto Child { get; set; }
        [JsonProperty("self")] public string Self { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
    }
}