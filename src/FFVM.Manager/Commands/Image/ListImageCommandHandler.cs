using FFVM.Base;
using FFVM.Base.Commands.BaseTypes;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.Exceptions;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Utility;
using FFVM.Manager.Commands.Image.Request;
using FFVM.Manager.Models.Enums;
using FFVM.Manager.Services.BaseTypes;
using FFVM.Manager.Services.Requests;
using FFVM.Manager.Utility;

namespace FFVM.Manager.Commands.Image;

public class ListImageCommandHandler(
    ProgramContext _context, 
    ILogger _logger,
    IConfigurationProvider _configurationProvider, 
    IFFUtilitiesSystemRepositoryServiceFactory _ffUtilitiesSystemRepositoryServiceFactory) : BaseCommandHandler<ListImageCommandRequest>(_context, _logger, _privateCommandName, _privateCommandDescription), ICommandHandler
{
    private const string _privateCommandName = "list";
    private const string _privateCommandDescription = "Lists the installed versions of FFmpeg/FFprobe utilities container, with container build ids.";

    public override async Task<int> Process()
    {
        Guard.AgainstEmptyConfiguration(_configurationProvider);
        Guard.AgainstNull(Parameters, nameof(Parameters));

        return Parameters?.CommandType switch
        {
            ListCommandType.Available => await ProcessListAvailableImages(),
            _ => await ProcessListInstalledImages(),
        };
    }

    private Task<int> ProcessListInstalledImages()
    {
        if (_configurationProvider.Configuration.Versions.Count == 0)
        {
            _logger.WriteStdErr("You currently have no installed images in your image store.");
            return Task.FromResult(ExitCodes.Successful);
        }

        _logger.WriteStdErr("");

        var tableWriter = new StdErrTableWriter();
        tableWriter.AddHeader(" "); // in-use indicator
        tableWriter.AddHeader("VERSION", _configurationProvider.Configuration.Versions.Max(v => v.Name.Length));
        tableWriter.AddHeader("EXE_VERSION");
        tableWriter.AddHeader("URL");

        var groupedInstalledVersions = _configurationProvider.Configuration.Versions
            .OrderBy(v => v.FFmpegVersion)
            .GroupBy(v => v.RepositoryId);

        foreach (var group in groupedInstalledVersions)
        {
            foreach (var installedVersion in group)
            {
                var inUseIndicator = installedVersion.IsSelected ? "*" : " ";
                tableWriter.AddRow(inUseIndicator, installedVersion.Name, installedVersion.FFmpegVersion, installedVersion.GetImageUrl());
            }
        }

        tableWriter.Write(_logger);

        return Task.FromResult(ExitCodes.Successful);
    }
    private async Task<int> ProcessListAvailableImages()
    {
        //we default the searched repository to the current selected repository.
        var matchingRepository = _configurationProvider.Configuration.Repositories.FirstOrDefault(r =>
            (!string.IsNullOrWhiteSpace(Parameters?.RepositoryName) && r.Name.Equals(Parameters.RepositoryName, StringComparison.OrdinalIgnoreCase))
         || (string.IsNullOrWhiteSpace(Parameters?.RepositoryName) && r.IsDefault))
            ?? throw new CommandWorkflowValidationException("There are no matching repositories in your repository store, add a repository and try again");

        var ffutilitiesRepositoryService = await _ffUtilitiesSystemRepositoryServiceFactory.GetFFUtilitiesSystemRepository(matchingRepository.Type)
            ?? throw new FFUtilitiesRepositoryServiceNotFoundException(matchingRepository.Type);

        var availableImagesResult = await ffutilitiesRepositoryService.GetImageVersions(new GetImageVersionsRequest { Repository = matchingRepository })
            ?? throw new CommandWorkflowValidationException($"There are no images available for the repository '{matchingRepository.Url}'.");

        _logger.WriteStdErr("");

        var tableWriter = new StdErrTableWriter();
        tableWriter.AddHeader("TAG", availableImagesResult.ImageVersions.Max(t => t.Tag!.Length));
        tableWriter.AddHeader("URL");

        var orderedImageVersions = availableImagesResult.ImageVersions.OrderByDescending(v => v.Tag);

        foreach (var imageVersion in orderedImageVersions)
        {
            tableWriter.AddRow(imageVersion.Tag!, imageVersion.Url!);
        }

        tableWriter.Write(_logger);

        return ExitCodes.Successful;
    }
}
