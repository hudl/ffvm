namespace FFVM.Base.Config;

public class ContainerRepository
{
    public bool IsDefault { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string AuthorizationId { get; set; } = string.Empty;
    public ContainerRepositoryType Type { get; set; } = ContainerRepositoryType.AwsEcr;
}
