using System.Security;

namespace FFVM.Manager.Models;

public class DockerHubAuthentication
{
    public string? Username { get; set; }
    public SecureString? PasswordOrToken { get; set; }
}
