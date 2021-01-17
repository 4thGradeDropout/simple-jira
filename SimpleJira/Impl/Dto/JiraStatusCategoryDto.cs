using Newtonsoft.Json;

namespace SimpleJira.Impl.Dto
{
    internal class JiraStatusCategoryDto
    {
        [JsonProperty("self")] public string Self { get; set; }
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("key")] public string Key { get; set; }
        [JsonProperty("colorName")] public string ColorName { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
    }
}