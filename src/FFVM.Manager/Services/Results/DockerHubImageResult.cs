using System.Text.Json.Serialization;

namespace FFVM.Manager.Services.Results;

public class DockerHubImageResult
{
    [JsonPropertyName("name")]
    public string? TagName { get; set; }
    [JsonPropertyName("tag_last_pulled")]
    public string? TagLastPullDate { get; set; }
    [JsonPropertyName("tag_last_pushed")]
    public string? TagLastPushDate { get; set; }
    [JsonPropertyName("status")]
    public string? Status { get; set; }
}
