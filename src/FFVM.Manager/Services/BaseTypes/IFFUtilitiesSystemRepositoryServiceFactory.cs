using FFVM.Base.Config;

namespace FFVM.Manager.Services.BaseTypes;

public interface IFFUtilitiesSystemRepositoryServiceFactory
{
    Task<IFFUtilitiesSystemRepositoryService?> GetFFUtilitiesSystemRepository(ContainerRepositoryType repositoryType);
}
