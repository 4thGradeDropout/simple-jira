using System;
using System.Text.Json.Serialization;

namespace SimpleJira.Impl.Dto
{
    internal class JiraCommentDto
    {
        [JsonPropertyName("author")] public JiraUserDto Author { get; set; }
        [JsonPropertyName("body")] public string Body { get; set; }
        [JsonPropertyName("created")] public DateTime Created { get; set; }
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("self")] public string Self { get; set; }
        [JsonPropertyName("updateAuthor")] public JiraUserDto UpdateAuthor { get; set; }
        [JsonPropertyName("updated")] public DateTime Updated { get; set; }
    }
}