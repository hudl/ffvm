using Amazon.Runtime;

namespace FFVM.Manager.Models;

public class AmazonEcrCredentials(AWSCredentials credentials)
{
    public AWSCredentials Credentials { get; } = credentials;
}
