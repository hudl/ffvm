using System.Runtime.InteropServices;
using FFVM.Base.Exceptions;
using FFVM.Base.Utility;
using FFVM.Manager.Services.BaseTypes;
using FFVM.Manager.Services.Requests;
using FFVM.Manager.Services.Results;
using FFVM.Manager.Utility;
using RestSharp;

namespace FFVM.Manager.Services;

public class DockerHubApiService : IDockerHubApiService
{
    public const string DockerHubV2RestApiBaseUrl = "https://hub.docker.com/v2";

    public async Task<string?> GetAuthenticationToken(GetAuthenticationTokenRequest getAuthenticationTokenRequest)
    {
        Guard.AgainstNull(getAuthenticationTokenRequest, nameof(getAuthenticationTokenRequest));
        Guard.AgainstEmptySecure(getAuthenticationTokenRequest.Password, nameof(getAuthenticationTokenRequest.Password));
        Guard.AgainstNullOrWhitespace(getAuthenticationTokenRequest.UserName, nameof(getAuthenticationTokenRequest.UserName));

        var passwordPtr = Marshal.SecureStringToBSTR(getAuthenticationTokenRequest.Password);
        var password = Marshal.PtrToStringBSTR(passwordPtr);
        Marshal.ZeroFreeBSTR(passwordPtr);

        var loginOptions = new RestClientOptions(DockerHubV2RestApiBaseUrl);
        var loginClient = new RestClient(loginOptions);
        var loginRequest = new RestRequest("users/login").AddJsonBody(new
        {
            username = getAuthenticationTokenRequest.UserName,
            password,
        });
        var loginResponse = await loginClient.PostAsync<DockerHubApiLoginResult>(loginRequest);
        return loginResponse?.AuthenticationToken ?? string.Empty;
    }
    public async Task<GetRepositoryTagsResult?> GetRepositoryTags(GetRepositoryTagsRequest getRepositoryTagsRequest)
    {
        Guard.AgainstNull(getRepositoryTagsRequest, nameof(getRepositoryTagsRequest));
        Guard.AgainstNullOrWhitespace(getRepositoryTagsRequest.Token, nameof(getRepositoryTagsRequest.Token));
        Guard.AgainstNullOrWhitespace(getRepositoryTagsRequest.RepositoryName, nameof(getRepositoryTagsRequest.RepositoryName));

        var repositoryNameParts = getRepositoryTagsRequest.RepositoryName!.Split("/");
        if (repositoryNameParts.Length != 2)
        {
            throw new CommandWorkflowValidationException($"The requested repository '{getRepositoryTagsRequest.RepositoryName}' has more than two parts in docker URL (expecting namespace/repository) got {getRepositoryTagsRequest.RepositoryName}.");
        }

        var getRepositoryTagsResult = new GetRepositoryTagsResult();
        try
        {
            var options = new RestClientOptions(DockerHubV2RestApiBaseUrl)
            {
                Authenticator = new DockerHubRestSharpAuthenticator(getRepositoryTagsRequest.Token)
            };
            var client = new RestClient(options);

            var currentPageNumber = 1;
            var moreResultsAreNeeded = true;
            while (moreResultsAreNeeded)
            {
                var dockerHubRepositoryTagsRequest = new RestRequest($"namespaces/{repositoryNameParts[0]}/repositories/{repositoryNameParts[1]}/tags")
                    .AddQueryParameter("page", currentPageNumber)
                    .AddQueryParameter("page_size", 100);

                var dockerHubRepositoryTagsResult = await client.GetAsync<DockerHubRepositoryTagsResult>(dockerHubRepositoryTagsRequest)
                    ?? throw new CommandWorkflowValidationException($"Failed to get tags for repository '{getRepositoryTagsRequest.RepositoryName}'");

                if (dockerHubRepositoryTagsResult.ImageResults?.Count == 0)
                {
                    break;
                }

                getRepositoryTagsResult.ImageVersions.AddRange(dockerHubRepositoryTagsResult.ImageResults!
                    .OrderBy(ir => ir.TagName)
                    .Select(ir => new ImageVersion
                    {
                        Url = $"{getRepositoryTagsRequest.RepositoryName}:{ir.TagName!}",
                        Tag = ir.TagName!
                    }));

                currentPageNumber++;
                moreResultsAreNeeded = string.IsNullOrWhiteSpace(dockerHubRepositoryTagsResult.NextPageKey);
            }
        }
        catch (CommandWorkflowValidationException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new CommandWorkflowValidationException($"Failed to get tags for repository '{getRepositoryTagsRequest.RepositoryName}'");
        }

        return getRepositoryTagsResult;
    }
}
