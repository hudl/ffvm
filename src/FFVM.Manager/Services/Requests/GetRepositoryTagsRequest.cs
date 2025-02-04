namespace FFVM.Manager.Services.Requests;

public class GetRepositoryTagsRequest(string repositoryName, string token)
{
    public string RepositoryName { get; set; } = repositoryName;
    public string Token { get; set; } = token;
}
