using FFVM.Base.Config;

namespace FFVM.Manager.Models;

public class AmazonEcrAuthentication
{
    public AmazonEcrAuthentication(string? awsProfile)
    {
        AwsProfile = awsProfile;
    }
    public AmazonEcrAuthentication(ContainerRepository containerRepository)
    {
        AwsProfile = containerRepository.AuthorizationId;
    }

    public string? AwsProfile { get; set; }
}
