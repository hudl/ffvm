namespace FFVM.Manager.Services.Results;

public class InstallImageResult
{
    public string? RegistryName { get; set; }
    public string? RepositoryName { get; set; } 
    public string? FFmpegVersion { get; set; } 
    public string? FFmpegPatch { get; set; } 
}
