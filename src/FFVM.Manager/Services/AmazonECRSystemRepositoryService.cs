using Amazon.ECR;
using Amazon.ECR.Model;
using FFVM.Base.Config;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.Exceptions;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Shell.BaseTypes;
using FFVM.Base.Shell.Models;
using FFVM.Base.Utility;
using FFVM.Manager.Models;
using FFVM.Manager.Services.BaseTypes;
using FFVM.Manager.Services.Requests;
using FFVM.Manager.Services.Results;
using System.Text;

namespace FFVM.Manager.Services;

public class AmazonEcrSystemRepositoryService(
    ILogger _logger, 
    IInputsProvider _inputsProvider, 
    ISystemShellRunner _systemShellRunner, 
    IConfigurationProvider _configurationProvider, 
    IContainerAuthenicationService<AmazonEcrAuthentication, AmazonEcrCredentials> _amazonEcrAuthenticationService) : BaseContainerRepositoryService(_logger, _inputsProvider, _systemShellRunner, _configurationProvider), IFFUtilitiesSystemRepositoryService
{
    public ContainerRepositoryType RepositoryType => ContainerRepositoryType.AwsEcr;

    public async Task<InstallImageResult?> InstallImage(InstallImageRequest request)
    {
        Guard.AgainstNull(request, nameof(request));
        Guard.AgainstNull(request.Repository, nameof(request.Repository));

        var loginResult = await _amazonEcrAuthenticationService.Login(new LoginRequest<AmazonEcrAuthentication> { Authentication = new(request.Repository!) })
            ?? throw new CommandWorkflowValidationException("Unable to login to AWS, try again later.");

        var amazonEcrClient = new AmazonECRClient(loginResult.CredentialsContainer?.Credentials, Amazon.RegionEndpoint.USEast1);
        var authTokenResponse = await amazonEcrClient.GetAuthorizationTokenAsync(new GetAuthorizationTokenRequest())
            ?? throw new CommandWorkflowValidationException($"Unable to get login authorization token from Amazon ECR.");

        var installationResult = new InstallImageResult();
        var awsUrlParts = request!.Repository?.Url.Split("/", StringSplitOptions.RemoveEmptyEntries);
        installationResult.RegistryName = awsUrlParts?.FirstOrDefault();
        installationResult.RepositoryName = awsUrlParts?.LastOrDefault();

        var data = Convert.FromBase64String(authTokenResponse.AuthorizationData.First().AuthorizationToken);
        var decodedString = Encoding.UTF8.GetString(data)[4..];

        var dockerLoginResults = _systemShellRunner.RunCommand($"docker login --username AWS --password-stdin {installationResult.RegistryName}", ShellCommandOptions.NoEcho(decodedString));
        if (dockerLoginResults.ExitCode > 0)
        {
            _logger.WriteStdErr($"The docker login command failed, console output:");
            _logger.WriteStdErr(dockerLoginResults.StdErr);
            throw new CommandWorkflowValidationException("The Docker login to AWS ECR failed failed.");
        }

        (installationResult.FFmpegVersion, installationResult.FFmpegPatch) = PullImageAndGetFFMpegVersion($"{request.Repository!.Url}:{request.ImageVersion}", request.EchoDockerPull);
        if (string.IsNullOrWhiteSpace(installationResult.FFmpegVersion))
        {
            throw new CommandWorkflowValidationException($"The docker pull and version detection failed");
        }

        return installationResult;
    }

    public async Task<ValidateRepositoryResult?> ValidateRepository(ValidateRepositoryRequest request)
    {
        Guard.AgainstNull(request, nameof(request));
        Guard.AgainstNullOrWhitespace(request.AwsProfile, nameof(request.AwsProfile));
        Guard.AgainstNullOrWhitespace(request.RepositoryUrl, nameof(request.RepositoryUrl));

        var respositoryNameParts = request.RepositoryUrl!.Split('/');
        if (respositoryNameParts.Length < 2)
        {
            throw new CommandWorkflowValidationException($"Could not validate ecr respository '{request.RepositoryUrl}'");
        }

        var ecrRepositoryFullParts = request.RepositoryUrl.Split(".");
        if (ecrRepositoryFullParts.Length < 2)
        {
            throw new CommandWorkflowValidationException($"Could not validate ecr respository '{request.RepositoryUrl}'");
        }

        var loginResult = await _amazonEcrAuthenticationService.Login(new LoginRequest<AmazonEcrAuthentication> { Authentication = new(request.AwsProfile) })
            ?? throw new CommandWorkflowValidationException("Unable to login to AWS, try again later.");

        var awsEcrRepositoryIsValid = false;
        try
        {
            var amazonEcrClient = new AmazonECRClient(loginResult.CredentialsContainer?.Credentials, Amazon.RegionEndpoint.USEast1);
            var describeRepositoriesResponse = await amazonEcrClient.DescribeRepositoriesAsync(new DescribeRepositoriesRequest
            {
                RegistryId = ecrRepositoryFullParts.FirstOrDefault(),
                RepositoryNames = [respositoryNameParts[1]],
            });
            if (describeRepositoriesResponse?.Repositories?.Count == 0)
            {
                throw new CommandWorkflowValidationException($"Could not validate ecr respository '{request.RepositoryUrl}'");
            }
            awsEcrRepositoryIsValid = true;
        }
        catch (CommandWorkflowValidationException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new CommandWorkflowValidationException($"Could not validate ecr respository '{request.RepositoryUrl}'");
        }

        return awsEcrRepositoryIsValid
            ? new ValidateRepositoryResult()
            : default;
    }

    public async Task<GetImageVersionsResult?> GetImageVersions(GetImageVersionsRequest request)
    {
        Guard.AgainstNull(request, nameof(request));
        Guard.AgainstNull(request.Repository, nameof(request.Repository));
        Guard.AgainstValuesOtherThan(request.Repository!.Type, ContainerRepositoryType.AwsEcr, nameof(request.Repository.Type));

        var loginResult = await _amazonEcrAuthenticationService.Login(new LoginRequest<AmazonEcrAuthentication> { Authentication = new(request.Repository) })
            ?? throw new CommandWorkflowValidationException("Unable to login to AWS, try again later.");

        var amazonEcrClient = new AmazonECRClient(loginResult.CredentialsContainer?.Credentials, Amazon.RegionEndpoint.USEast1);

        try
        {
            var ecrRepositoryFullParts = request.Repository.Url.Split(".");
            var ecrRepositoryUrlParts = request.Repository.Url.Split("/");
            var ecrRepositoryName = ecrRepositoryUrlParts.LastOrDefault();
            var describeImagesResult = await amazonEcrClient.ListImagesAsync(new ListImagesRequest
            {
                RegistryId = ecrRepositoryFullParts.FirstOrDefault(),
                RepositoryName = ecrRepositoryName,
            });
            if (describeImagesResult?.ImageIds == null)
            {
                throw new CommandWorkflowValidationException($"Could not load images from respository '{request.Repository.Name}'");
            }

            var imageIds = describeImagesResult.ImageIds
                .GroupBy(i => i.ImageDigest)
                .Select(i => i.MinBy(i => i.ImageTag.Length));
            return new GetImageVersionsResult
            {
                ImageVersions = imageIds.Select(iid => new ImageVersion
                {
                    Tag = iid!.ImageTag,
                    Url = $"{request.Repository.Url}:{iid.ImageTag}"
                }).ToList()
            };
        }
        catch (CommandWorkflowValidationException)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
