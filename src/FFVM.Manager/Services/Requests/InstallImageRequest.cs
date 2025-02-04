using FFVM.Base.Config;

namespace FFVM.Manager.Services.Requests;

public class InstallImageRequest
{
    public ContainerRepository? Repository { get; set; }
    public string? ImageVersion { get; set; }
    public bool EchoDockerPull { get; set; }
}
