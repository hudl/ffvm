using FFVM.Base.Config;
using FFVM.Base.Utility;

namespace FFVM.Manager.Commands.Repository.Request;

public class AddRepositoryCommandRequest
{
    [CommandParameter("url", order: 1, description: "The url to repository where the docker containers live.", isrequired: true)]
    public string? Url { get; set; }

    [CommandParameter("name", order: 2, description: "An optional short name for the repository, name defaults to full URL if not provided.", isrequired: true)]
    public string? Name { get; set; }

    [CommandParameter("default", description: "Specifies that this will be the default repository from which to pull images, unless otherwise specified.", isvalueless: false)]
    public bool MakeDefault { get; set; }

    [CommandParameter("type", description: "The type of repository, current supported values are (DOCKER_HUB, AWS_ECR), default is auto-assumed from URL", customFormatterType: typeof(EnumParameterFormatter<ContainerRepositoryType>))]
    public ContainerRepositoryType Type { get; set; }

    [CommandParameter("profile", description: "For repository type AWS_ECR, the aws profile for authentication.")]
    public string? AwsProfile { get; set; }

    [CommandParameter("user", description: "For repository type DOCKER_HUB, the docker hub username for authentication.")]
    public string? DockerHubUsername { get; set; }

    [CommandParameter("password", description: "For repository type DOCKER_HUB, the docker hub password for authentication.")]
    public string? DockerHubAccessToken { get; set; }
}
