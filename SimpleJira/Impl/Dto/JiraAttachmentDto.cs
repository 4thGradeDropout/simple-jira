using System;
using Newtonsoft.Json;

namespace SimpleJira.Impl.Dto
{
    internal class JiraAttachmentDto
    {
        [JsonProperty("author")] public JiraUserDto Author { get; set; }
        [JsonProperty("content")] public string Content { get; set; }
        [JsonProperty("created")] public DateTime Created { get; set; }
        [JsonProperty("filename")] public string Filename { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("self")] public string Self { get; set; }
        [JsonProperty("size")] public int Size { get; set; }
    }
}