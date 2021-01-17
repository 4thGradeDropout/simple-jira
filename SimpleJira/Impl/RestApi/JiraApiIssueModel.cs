using System.Collections.Generic;
using Newtonsoft.Json;

namespace SimpleJira.Impl.RestApi
{
    internal class JiraApiIssueModel
    {
        [JsonProperty("expand")] public string Expand { get; set; }
        [JsonProperty("key")] public string Key { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("self")] public string Self { get; set; }
        [JsonProperty("fields")] public Dictionary<string, object> Fields { get; set; }
    }
}