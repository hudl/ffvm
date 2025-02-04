using FFVM.Base.Config;
using FFVM.Manager.Services.Requests;
using FFVM.Manager.Services.Results;

namespace FFVM.Manager.Services.BaseTypes;

public interface IFFUtilitiesSystemRepositoryService
{
    ContainerRepositoryType RepositoryType { get; }
    Task<ValidateRepositoryResult?> ValidateRepository(ValidateRepositoryRequest request);
    Task<InstallImageResult?> InstallImage(InstallImageRequest request);
    Task<GetImageVersionsResult?> GetImageVersions(GetImageVersionsRequest request);
}
