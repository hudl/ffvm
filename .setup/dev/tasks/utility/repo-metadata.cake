#load "./configuration.cake"
#load "./cake-context-accessor.cake"

using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// This class is a centralized location for project information determined from file structure or other
/// repo content.
/// </summary>
public class RepoMetadata
{
    public RepoMetadata(string repoRoot)
    {
        RepoRootPath = new System.IO.DirectoryInfo(repoRoot).FullName;
        SourcePath = System.IO.Path.Combine(RepoRootPath, "src");
        SetupPath = System.IO.Path.Combine(RepoRootPath, ".setup");
        RuntimeId = GetRuntimeIdentifier();
        BuildPath = System.IO.Path.Combine(SetupPath, "build", RuntimeId);

        var homeEnvVariable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "USERPROFILE" : "HOME";
        HomePath = System.Environment.GetEnvironmentVariable(homeEnvVariable);
        InstallPath = System.IO.Path.Combine(HomePath, ".local", "bin");

        ConfigurationPath = System.IO.Path.Combine(HomePath, ".ffvm");
        ConfigurationScriptPath = System.IO.Path.Combine(RepoRootPath, "configuration.json");
    }

    public string RuntimeId { get; }
    public string RepoRootPath { get; }
    public string SetupPath { get; }
    public string BuildPath { get; }
    public string SourcePath { get; }
    public string InstallPath { get; }
    public string ConfigurationPath { get; }
    public string HomePath { get; }
    public string ConfigurationScriptPath { get; }

    private static string GetRuntimeIdentifier()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "win-x64";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "osx-arm64";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "linux-x64";
        return "osx-arm64"; //most developers at Hudl are developing with M-series MACBooks
    }

#nullable enable
    private Lazy<CustomConfiguration?> _configurationScript = new Lazy<CustomConfiguration?>(() => ConfigurationHelper.LoadCustomConfiguration());  
    public CustomConfiguration? ConfigurationScript => _configurationScript.Value; 
#nullable disable 

    private static Lazy<RepoMetadata> _instance = new Lazy<RepoMetadata>(() =>
        new RepoMetadata($"{CakeContextAccessor.Context.Environment.WorkingDirectory.FullPath}/../.."));

    public static RepoMetadata Current => _instance.Value;
}