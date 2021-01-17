using Newtonsoft.Json;

namespace SimpleJira.Impl.Dto
{
    internal class JiraAvatarUrlsDto
    {
        [JsonProperty("16x16")] public string Size16x16 { get; set; }
        [JsonProperty("24x24")] public string Size24x24 { get; set; }
        [JsonProperty("32x32")] public string Size32x32 { get; set; }
        [JsonProperty("48x48")] public string Size48x48 { get; set; }
    }
}