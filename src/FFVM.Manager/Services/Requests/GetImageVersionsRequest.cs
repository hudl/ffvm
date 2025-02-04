using FFVM.Base.Config;

namespace FFVM.Manager.Services.Requests;

public class GetImageVersionsRequest
{
    public ContainerRepository? Repository { get; set; }
}
