using System.Text.Json.Serialization;

namespace FFVM.Manager.Services.Results;

public class DockerHubApiLoginResult
{
    [JsonPropertyName("token")]
    public string? AuthenticationToken { get; set; }
}
