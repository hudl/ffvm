using FFVM.Base.Config;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.Exceptions;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Shell.BaseTypes;
using FFVM.Base.Utility;
using FFVM.Manager.Models;
using FFVM.Manager.Services.BaseTypes;
using FFVM.Manager.Services.Requests;
using FFVM.Manager.Services.Results;

namespace FFVM.Manager.Services;

public partial class DockerHubSystemRepositoryService(
    ILogger _logger, 
    IInputsProvider _inputsProvider, 
    ISystemShellRunner _systemShellRunner, 
    IConfigurationProvider _configurationProvider, 
    IContainerAuthenicationService<DockerHubAuthentication, DockerHubCredentials> _containerAuthenticationService, 
    IDockerHubApiService _dockerHubApiService) : BaseContainerRepositoryService(_logger, _inputsProvider, _systemShellRunner, _configurationProvider), IFFUtilitiesSystemRepositoryService
{
    public ContainerRepositoryType RepositoryType => ContainerRepositoryType.DockerHub;

    public async Task<InstallImageResult?> InstallImage(InstallImageRequest request)
    {
        Guard.AgainstNull(request, nameof(request));
        Guard.AgainstNull(request.Repository, nameof(request.Repository));

        _ = await _containerAuthenticationService.Login(new LoginRequest<DockerHubAuthentication>())
            ?? throw new CommandWorkflowValidationException("Unable to login to the Docker, try again later.");

        var installationResult = new InstallImageResult();
        var awsUrlParts = request.Repository!.Url.Split("/", StringSplitOptions.RemoveEmptyEntries);
        installationResult.RegistryName = awsUrlParts.FirstOrDefault();
        installationResult.RepositoryName = awsUrlParts.LastOrDefault();

        (installationResult.FFmpegVersion, installationResult.FFmpegPatch) = PullImageAndGetFFMpegVersion($"{request.Repository.Url}:{request.ImageVersion}", request.EchoDockerPull);
        if (string.IsNullOrWhiteSpace(installationResult.FFmpegVersion))
        {
            throw new CommandWorkflowValidationException("The docker pull and version detection failed");
        }

        return installationResult;
    }

    public async Task<ValidateRepositoryResult?> ValidateRepository(ValidateRepositoryRequest request)
    {
        Guard.AgainstNull(request, nameof(request));
        Guard.AgainstNullOrWhitespace(request.RepositoryUrl, nameof(request.RepositoryUrl));

        var repositoryNameParts = request.RepositoryUrl!.Split("/");
        if (repositoryNameParts.Length != 2)
        {
            throw new CommandWorkflowValidationException($"The requested repository '{request.RepositoryUrl}' has more than two parts in docker URL (expecting namespace/repository) got {request.RepositoryUrl}.");
        }

        var dockerLoginResult = await _containerAuthenticationService.Login(new LoginRequest<DockerHubAuthentication>())
            ?? throw new CommandWorkflowValidationException("Unable to login to the Docker, try again later.");

        var dockerRepositoryTagsRequest = new GetRepositoryTagsRequest(request.RepositoryUrl, dockerLoginResult.CredentialsContainer!.Token);
        var dockerRepositoryTagsResults = await _dockerHubApiService.GetRepositoryTags(dockerRepositoryTagsRequest);
        return dockerRepositoryTagsResults?.ImageVersions.Count > 0
            ? new ValidateRepositoryResult()
            : default;
    }

    public async Task<GetImageVersionsResult?> GetImageVersions(GetImageVersionsRequest request)
    {
        Guard.AgainstNull(request, nameof(request));
        Guard.AgainstNull(request.Repository, nameof(request.Repository));
        Guard.AgainstValuesOtherThan(request.Repository!.Type, ContainerRepositoryType.DockerHub, nameof(request.Repository.Type));

        var dockerLoginResult = await _containerAuthenticationService.Login(new LoginRequest<DockerHubAuthentication>())
            ?? throw new CommandWorkflowValidationException("Unable to login to the Docker, try again later.");

        var dockerRepositoryTagsRequest = new GetRepositoryTagsRequest(request.Repository.Url, dockerLoginResult.CredentialsContainer!.Token);
        var dockerRepositoryTagsResults = await _dockerHubApiService.GetRepositoryTags(dockerRepositoryTagsRequest);
        return new GetImageVersionsResult
        {
            ImageVersions = dockerRepositoryTagsResults?.ImageVersions?.OrderBy(iv => iv.Tag)?.ToList() ?? []
        };
    }
}
