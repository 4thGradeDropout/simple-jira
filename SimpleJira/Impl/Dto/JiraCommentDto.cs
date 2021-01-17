using System;
using Newtonsoft.Json;

namespace SimpleJira.Impl.Dto
{
    internal class JiraCommentDto
    {
        [JsonProperty("author")] public JiraUserDto Author { get; set; }
        [JsonProperty("body")] public string Body { get; set; }
        [JsonProperty("created")] public DateTime Created { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("self")] public string Self { get; set; }
        [JsonProperty("updateAuthor")] public JiraUserDto UpdateAuthor { get; set; }
        [JsonProperty("updated")] public DateTime Updated { get; set; }
    }
}