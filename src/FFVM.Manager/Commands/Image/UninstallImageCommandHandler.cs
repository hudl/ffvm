using FFVM.Base;
using FFVM.Base.Commands.BaseTypes;
using FFVM.Base.Config.BaseTypes;
using FFVM.Base.Exceptions;
using FFVM.Base.IO.BaseTypes;
using FFVM.Base.Utility;
using FFVM.Manager.Commands.Image.Request;

namespace FFVM.Manager.Commands.Image;

public class UninstallImageCommandHandler(
    ProgramContext _context, 
    ILogger _logger, 
    IConfigurationProvider _configurationProvider) : BaseCommandHandler<RemoveImageCommandRequest>(_context, _logger, _privateCommandName, _privateCommandDescription), ICommandHandler
{
    private const string _privateCommandName = "uninstall";
    private const string _privateCommandDescription = "Removes a specified version of FFmpeg/FFprobe utilities container.";

    public override async Task<int> Process()
    {
        Guard.AgainstEmptyConfiguration(_configurationProvider);
        Guard.AgainstNull(Parameters, nameof(Parameters));
        Guard.AgainstNullOrWhitespace(Parameters?.Name, nameof(Parameters.Name));

        var matchingRepository = _configurationProvider.Configuration.Versions.FirstOrDefault(r => r.Name == Parameters?.Name)
            ?? throw new CommandWorkflowValidationException($"The requested image '{Parameters?.Name}', does not exist in the image store");

        _configurationProvider.Configuration.Versions.RemoveAll(r => r.Name == Parameters?.Name);

        await _configurationProvider.Save();

        _logger.WriteStdOut($"Successfully uninstalled image '{Parameters?.Name}'.");

        return ExitCodes.Successful;
    }
}
