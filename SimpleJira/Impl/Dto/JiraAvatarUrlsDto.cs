using System.Text.Json.Serialization;

namespace SimpleJira.Impl.Dto
{
    internal class JiraAvatarUrlsDto
    {
        [JsonPropertyName("16x16")] public string Size16x16 { get; set; }
        [JsonPropertyName("24x24")] public string Size24x24 { get; set; }
        [JsonPropertyName("32x32")] public string Size32x32 { get; set; }
        [JsonPropertyName("48x48")] public string Size48x48 { get; set; }
    }
}