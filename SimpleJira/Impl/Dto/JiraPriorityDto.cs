using Newtonsoft.Json;

namespace SimpleJira.Impl.Dto
{
    internal class JiraPriorityDto
    {
        [JsonProperty("iconUrl")] public string IconUrl { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("self")] public string Self { get; set; }
    }
}