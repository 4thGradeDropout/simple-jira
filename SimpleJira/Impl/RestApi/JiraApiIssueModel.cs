using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleJira.Impl.RestApi
{
    internal class JiraApiIssueModel
    {
        [JsonPropertyName("expand")] public string Expand { get; set; }
        [JsonPropertyName("key")] public string Key { get; set; }
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("self")] public string Self { get; set; }
        [JsonPropertyName("fields")] public Dictionary<string, object> Fields { get; set; }
    }
}