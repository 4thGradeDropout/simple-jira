using System;
using System.Text.Json.Serialization;
namespace SimpleJira.Impl.Dto
{
    internal class JiraAttachmentDto
    {
        [JsonPropertyName("author")] public JiraUserDto Author { get; set; }
        [JsonPropertyName("content")] public string Content { get; set; }
        [JsonPropertyName("created")] public DateTime Created { get; set; }
        [JsonPropertyName("filename")] public string Filename { get; set; }
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("self")] public string Self { get; set; }
        [JsonPropertyName("size")] public int Size { get; set; }
    }
}