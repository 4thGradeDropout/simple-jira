using Newtonsoft.Json;

namespace SimpleJira.Impl.Dto
{
    internal class JiraStatusDto
    {
        [JsonProperty("self")] public string Self { get; set; }
        [JsonProperty("description")] public string Description { get; set; }
        [JsonProperty("iconUrl")] public string IconUrl { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("statusCategory")] public JiraStatusCategoryDto StatusCategory { get; set; }
    }
}