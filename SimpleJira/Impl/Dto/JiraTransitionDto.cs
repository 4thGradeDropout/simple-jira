using Newtonsoft.Json;

namespace SimpleJira.Impl.Dto
{
    internal class JiraTransitionDto
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("to")] public JiraStatusDto To { get; set; }
    }
}