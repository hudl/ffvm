namespace FFVM.Manager.Models;

public class DockerHubCredentials(string token)
{
    public string Token { get; } = token;
}
