using FFVM.Base.Config;
using FFVM.Base.Utility;

namespace FFVM.Base.Exceptions;

public class FFUtilitiesRepositoryServiceNotFoundException(ContainerRepositoryType repositoryType) 
    : CommandWorkflowValidationException($"The requested repository type '{repositoryType}', is not a n approved type, valid types are ({string.Join(",", EnumMappingUtility.GetEnumMappings<ContainerRepositoryType>().Select(m => m.MappingAttribute!.Value))})")
{
}
