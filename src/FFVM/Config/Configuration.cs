using FFVM.Base.Utility;

namespace FFVM.Base.Config;

public class Configuration
{
    [SetConfigurationParameter("installation-path")]
    public string InstallationPath { get; set; } = string.Empty;
    [SetConfigurationParameter("sso-profile")]
    public string SSOProfileName { get; set; } = string.Empty;
    public List<AuthorizationRecord> Authorizations { get; set; } = [];
    public List<ContainerRepository> Repositories { get; set; } = [];
    public List<ManagedVersion> Versions { get; set; } = [];
}
