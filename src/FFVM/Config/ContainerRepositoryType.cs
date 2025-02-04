using FFVM.Base.Utility;

namespace FFVM.Base.Config;

public enum ContainerRepositoryType
{
    Unknown = 0,
    [Mapping("AWS_ECR")]
    AwsEcr = 1, 
    [Mapping("DOCKER_HUB")]
    DockerHub = 2,
}
