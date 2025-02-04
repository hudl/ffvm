namespace FFVM.Manager.Services.Requests;

public class ValidateRepositoryRequest
{
    public string? AwsProfile { get; set; }
    public string? DockerHubUsername { get; set; }
    public string? DockerHubAccessToken { get; set; }
    public string? RepositoryUrl { get; set; }
}
