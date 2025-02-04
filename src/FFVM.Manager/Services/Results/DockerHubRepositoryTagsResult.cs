using System.Text.Json.Serialization;

namespace FFVM.Manager.Services.Results;

public class DockerHubRepositoryTagsResult
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("next")]
    public string? NextPageKey { get; set; }
    [JsonPropertyName("previous")]
    public string? PreviousPageKey { get; set; }
    [JsonPropertyName("results")]
    public List<DockerHubImageResult>? ImageResults { get; set; }
}
