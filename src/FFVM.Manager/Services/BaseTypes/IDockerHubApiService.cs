using FFVM.Manager.Services.Requests;
using FFVM.Manager.Services.Results;

namespace FFVM.Manager.Services.BaseTypes;

public interface IDockerHubApiService
{
    Task<string?> GetAuthenticationToken(GetAuthenticationTokenRequest getAuthenticationTokenRequest);
    Task<GetRepositoryTagsResult?> GetRepositoryTags(GetRepositoryTagsRequest getRepositoryTagsRequest);
}
