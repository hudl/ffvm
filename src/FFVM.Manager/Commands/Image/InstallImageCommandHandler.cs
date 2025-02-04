using FFVM.Base;
using FFVM.Base.Commands.BaseTypes;
using FFVM.Base.Config;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.Exceptions;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Utility;
using FFVM.Manager.Commands.Image.Request;
using FFVM.Manager.Services.BaseTypes;
using FFVM.Manager.Services.Requests;
using FFVM.Manager.Services.Results;

namespace FFVM.Manager.Commands.Image;

public class InstallImageCommandHandler(
    ProgramContext _context, 
    ILogger _logger, 
    IConfigurationProvider _configurationProvider, 
    IFFUtilitiesSystemRepositoryServiceFactory _ffUtilitiesSystemRepositoryServiceFactory) : BaseCommandHandler<InstallImageCommandRequest>(_context, _logger, _privateCommandName, _privateCommandDescription), ICommandHandler
{
    private const string _privateCommandName = "install";
    private const string _privateCommandDescription = "Installs latest, or a specified version of FFmpeg/FFprobe utilities container.";
    private const string _containerLatestIdentifier = "latest";

    public override async Task<int> Process()
    {
        Guard.AgainstEmptyConfiguration(_configurationProvider);
        Guard.AgainstNull(Parameters, nameof(Parameters));
        Guard.AgainstNullOrWhitespace(Parameters?.Version, nameof(Parameters.Version));

        var (repositoryName, version) = ParseRepositoryVersionFromParameters();

        var matchingVersion = _configurationProvider.Configuration.Versions.FirstOrDefault(v => v.ImageVersion.Equals(version, StringComparison.OrdinalIgnoreCase) && v.RepositoryId.Equals(repositoryName, StringComparison.OrdinalIgnoreCase));
        if (matchingVersion != null)
        {
            throw new CommandWorkflowValidationException($"The requested version '{Parameters?.Version}' is already installed, '{matchingVersion.RepositoryId}:{matchingVersion.ImageVersion}'");
        }

        var matchingRepository = _configurationProvider.Configuration.Repositories.FirstOrDefault(r => r.Name.Equals(repositoryName, StringComparison.OrdinalIgnoreCase))
            ?? throw new CommandWorkflowValidationException("There are no matching repositories in your repository store, add a repository and try again");

        var ffutilitiesRepositoryService = await _ffUtilitiesSystemRepositoryServiceFactory.GetFFUtilitiesSystemRepository(matchingRepository.Type)
            ?? throw new FFUtilitiesRepositoryServiceNotFoundException(matchingRepository.Type);

        var installationResult = await ffutilitiesRepositoryService.InstallImage(new InstallImageRequest
        {
            Repository = matchingRepository,
            ImageVersion = version,
            EchoDockerPull = true,
        })
            ?? throw new CommandWorkflowValidationException($"Failed to install image '{matchingRepository.Url}:{version}'");

        var versionName = GenerateVersionName(matchingRepository, installationResult);
        var versionWithSameName = _configurationProvider.Configuration.Versions.FirstOrDefault(v => v.Name == versionName);
        if (versionWithSameName != null && version.Equals(_containerLatestIdentifier, StringComparison.OrdinalIgnoreCase))
        {
            throw new CommandWorkflowValidationException($"The latest version of the image is already installed with name '{versionName}'");
        }
        if (versionWithSameName != null)
        {
            throw new CommandWorkflowValidationException($"The version alias '{versionName}' is already in use");
        }

        var managedVersion = new ManagedVersion
        {
            IsSelected = false,
            Name = versionName,
            ImageVersion = version,
            RepositoryId = matchingRepository.Name,
            FFmpegPatch = installationResult.FFmpegPatch ?? string.Empty,
            FFmpegVersion = installationResult.FFmpegVersion ?? string.Empty,
            RegistryName = installationResult.RegistryName ?? string.Empty,
            RepositoryName = installationResult.RepositoryName ?? string.Empty,
        };
        _configurationProvider.Configuration.Versions.Add(managedVersion);

        await _configurationProvider.Save();

        _logger.WriteStdOut($"Successfully installed '{managedVersion.GetImageUrl()}' and tagged with name '{versionName}'.");

        return ExitCodes.Successful;
    }

    private (string repositoryName, string version) ParseRepositoryVersionFromParameters()
    {
        var versionParts = Parameters?.Version?.Split(":", StringSplitOptions.RemoveEmptyEntries);
        var versionIdentifier = versionParts?.Length > 0 ? versionParts.Last() : _containerLatestIdentifier;
        var repositoryName = _configurationProvider.Configuration.Repositories.FirstOrDefault(r => versionParts?.Length > 1
            ? r.Name.Equals(versionParts.First(), StringComparison.OrdinalIgnoreCase)
            : r.IsDefault)?.Name ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(Parameters?.RepositoryName))
        {
            repositoryName = Parameters.RepositoryName;
        }

        return (repositoryName, versionIdentifier);
    }
    private string GenerateVersionName(ContainerRepository matchingRepository, InstallImageResult? installationResult)
    {
        if (!string.IsNullOrWhiteSpace(Parameters?.Name))
        {
            return Parameters.Name;
        }

        var versionNameSuffix = matchingRepository.Name != matchingRepository.Url
            ? $"_{matchingRepository.Name}"
            : $"_{matchingRepository.Type.ToString().ToLower()}";

        var versionName = $"{installationResult?.FFmpegVersion}{versionNameSuffix}";


        return versionName;
    }
}
