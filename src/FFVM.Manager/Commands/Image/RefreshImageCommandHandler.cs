using FFVM.Base;
using FFVM.Base.Commands.BaseTypes;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.Exceptions;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Utility;
using FFVM.Manager.Commands.Image.Request;
using FFVM.Manager.Services.BaseTypes;
using FFVM.Manager.Services.Requests;

namespace FFVM.Manager.Commands.Image;

public class RefreshImageCommandHandler(
    ProgramContext _context, 
    ILogger _logger, 
    IConfigurationProvider _configurationProvider, 
    IFFUtilitiesSystemRepositoryServiceFactory _ffUtilitiesSystemRepositoryServiceFactory) : BaseCommandHandler<RefreshImageCommandRequest>(_context, _logger, _privateCommandName, _privateCommandDescription), ICommandHandler
{
    private const string _privateCommandName = "refresh";
    private const string _privateCommandDescription = "Refreshes the container resource on the system, useful if the image was removed.";

    public override async Task<int> Process()
    {
        Guard.AgainstEmptyConfiguration(_configurationProvider);

        var matchingVersion = _configurationProvider.Configuration.Versions.FirstOrDefault(v => v.IsSelected)
            ?? throw new CommandWorkflowValidationException($"There are currently no selected images to refresh, select an image with 'use' and try again.");

        var matchingRepository = _configurationProvider.Configuration.Repositories.FirstOrDefault(r => r.Name.Equals(matchingVersion.RepositoryId, StringComparison.OrdinalIgnoreCase))
            ?? throw new CommandWorkflowValidationException("There are no matching repositories in your repository store, for this image re-add the image repository and try again");

        var ffutilitiesRepositoryService = await _ffUtilitiesSystemRepositoryServiceFactory.GetFFUtilitiesSystemRepository(matchingRepository.Type)
            ?? throw new FFUtilitiesRepositoryServiceNotFoundException(matchingRepository.Type);

        var installationResult = await ffutilitiesRepositoryService.InstallImage(new InstallImageRequest
        {
            Repository = matchingRepository,
            ImageVersion = matchingVersion.ImageVersion,
            EchoDockerPull = true,
        })
            ?? throw new CommandWorkflowValidationException($"Failed to refresh image '{matchingRepository.Url}:{matchingVersion.ImageVersion}'");

        _logger.WriteStdOut($"Successfully refreshed image '{matchingVersion.GetImageUrl()}'.");

        return ExitCodes.Successful;
    }

}
