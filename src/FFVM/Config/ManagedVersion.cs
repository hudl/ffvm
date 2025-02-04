namespace FFVM.Base.Config;

public class ManagedVersion
{
    public bool IsSelected { get; set; }
    public string RegistryName { get; set; } = string.Empty;
    public string RepositoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageVersion { get; set; } = string.Empty;
    public string FFmpegVersion { get; set; } = string.Empty;
    public string FFmpegPatch { get; set; } = string.Empty;
    public string RepositoryId { get; set; } = string.Empty;

    public string GetRepositoryUrl() => $"{RegistryName}/{RepositoryName}";
    public string GetImageUrl() => $"{GetRepositoryUrl()}:{ImageVersion}";
}
