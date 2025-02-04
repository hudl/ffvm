using FFVM.Base.Config;
using FFVM.Manager.Services.BaseTypes;

namespace FFVM.Manager.Services;

public class SystemRepositoryServiceFactory(IEnumerable<IFFUtilitiesSystemRepositoryService> repositoryServices) : IFFUtilitiesSystemRepositoryServiceFactory
{
    private readonly List<IFFUtilitiesSystemRepositoryService> _repositoryServices = repositoryServices.ToList();

    public Task<IFFUtilitiesSystemRepositoryService?> GetFFUtilitiesSystemRepository(ContainerRepositoryType repositoryType) => Task.FromResult(_repositoryServices.FirstOrDefault(rs => rs.RepositoryType == repositoryType));
}
